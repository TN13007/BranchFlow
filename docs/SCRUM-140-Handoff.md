# Bàn giao sửa lỗi SCRUM-140 — Đăng nhập lại sau phản hồi 401 tạo đơn báo form hết hạn lần đầu

Em là Công Thành, có công cụ hỗ trợ, đã xác nhận nguyên nhân trên một origin sạch, sửa lỗi và kiểm chứng tự động cho **SCRUM-140** (liên kết `SCRUM-109` / `SCRUM-132`).

## 1. Thông tin chung
- **Jira Issue**: [`SCRUM-140`](https://khangba153.atlassian.net/browse/SCRUM-140) — `[BUG] Đăng nhập lại sau phản hồi 401 tạo đơn báo form hết hạn lần đầu`
- **Người thực hiện**: Nguyễn Công Thành (FE)
- **Ngày thực hiện & kiểm chứng**: `18/11/2026` (Asia/Saigon)
- **Nền tích hợp (`origin/main`)**: `8f56909e3088f2d9712ef65f5ddaab11275d64ea`
- **Nhánh sửa lỗi**: `fix/SCRUM-140-relogin-form-expired` (đồng bộ sang nhánh cá nhân `thanhnc`)

---

## 2. Xác nhận nguyên nhân gốc (Root Cause Analysis trên một origin sạch)
Qua tái hiện trên một origin sạch duy nhất (`https://localhost:7133`), lỗi **không phải do xung đột nhiều cổng `localhost`**, mà là lỗi thật trong cơ chế điều hướng Blazor Interactive Server kết hợp với kiểm tra Antiforgery khi phiên `WebSessionStore` vừa bị hủy sau phản hồi `401`:

1. **Điều hướng nội bộ trong SignalR circuit khi nhấn `"Đăng nhập lại"`**:
   - Trong `App.razor`, toàn bộ `<Routes @rendermode="InteractiveServer" />` chạy ở chế độ Interactive Server.
   - Khi `POST /api/orders` (hoặc bất kỳ API nào) trả về `401 Unauthorized`, `SessionApiTransport.ReadAsync` xóa ngay khóa phiên trong `WebSessionStore` (`store.Remove(key)`), và `ApiError.razor` hiển thị liên kết `<a href="/login" data-enhance-nav="false">Đăng nhập lại</a>`.
   - Trong cây component Interactive Server, thuộc tính `data-enhance-nav="false"` không ngăn được bộ định tuyến SPA của Blazor chặn sự kiện click `<a href="/login">`. Do đó trình duyệt **không gửi HTTP `GET /login` mới**, mà chuyển sang `Login.razor` ngay bên trong SignalR circuit cũ:
     - Trình duyệt vẫn giữ cookie `__Host-BranchFlow.Auth` (của phiên vừa bị xóa khỏi `WebSessionStore`).
     - `<EditForm>` trong `Login.razor` lấy lại `__RequestVerificationToken` từ `AntiforgeryStateProvider` của circuit cũ (được đóng dấu `ClaimUid / Identity.Name = <username>` từ lúc vào trang đã đăng nhập, độ dài `198` ký tự thay vì `155` ký tự của anonymous).
2. **Xung đột danh tính (`ClaimsPrincipal`) khi gửi `POST /auth/login`**:
   - Khi người dùng nhấn **Đăng nhập** lần đầu (`POST /auth/login`), middleware `UseAuthentication()` chạy `CookieAuthenticationEvents.OnValidatePrincipal`.
   - `OnValidatePrincipal` thấy phiên trong `WebSessionStore` đã bị xóa nên gọi `context.RejectPrincipal()`, biến `HttpContext.User` thành **anonymous (`""`)** trước khi vào `WebAuthEndpoints.LoginAsync`.
   - Khi `LoginAsync` gọi `antiforgery.ValidateRequestAsync(context)`, ASP.NET Core so sánh token form (mang danh tính user cũ) với `HttpContext.User` hiện tại (anonymous `""`) $\rightarrow$ ném `AntiforgeryValidationException` và trả về `400 Bad Request` (`"Form đã hết hạn. Vui lòng tải lại trang."`).

---

## 3. Các thay đổi đã thực hiện (3 lớp bảo vệ)

1. **`src/BranchFlow.Web/Components/Orders/ApiError.razor`**:
   - Bổ sung `@onclick="Relogin" @onclick:preventDefault="true"` gọi `Navigation.NavigateTo("/login?notice=session", forceLoad: true)` cho nút **Đăng nhập lại**. Khi nhấn, trình duyệt thực hiện HTTP `GET /login?notice=session` thật để xóa ngay cookie `__Host-BranchFlow.Auth` hết hạn và cấp mới `<AntiforgeryToken />` anonymous (`len=155`).
2. **`src/BranchFlow.Web/Components/Pages/Login.razor`**:
   - Trong `OnAfterRenderAsync(bool firstRender)`, trước khi mở khóa nhập liệu (`ready = true`), kiểm tra nếu `AuthenticationState` của circuit vẫn đang `IsAuthenticated == true` nhưng `WebSessionStore.Find(key)` là `null` (trường hợp đi vào `/login` qua bất kỳ liên kết SPA nào khác hoặc nút Back), tự động gọi `Navigation.NavigateTo("/login?notice=session", forceLoad: true)`.
3. **`src/BranchFlow.Web/Program.cs`, `WebSessionStore.cs` & `WebAuthEndpoints.cs`**:
   - Trong `OnValidatePrincipal`, khi một cookie `__Host-BranchFlow.Auth` hợp lệ chữ ký bị từ chối do phiên trong `WebSessionStore` vừa hết hạn/bị xóa, lưu lại `ClaimsPrincipal` vừa bị từ chối vào `HttpContext.Items[WebSessionStore.RejectedPrincipalKey]`.
   - Trong `WebAuthEndpoints.ValidateAntiforgeryAsync` (dùng cho cả `/auth/login` và `/auth/logout`), nếu `ValidateRequestAsync` thất bại với `HttpContext.User` anonymous nhưng request có kèm `RejectedPrincipalKey`, thử xác thực lại token antiforgery đối chiếu với đúng `ClaimsPrincipal` vừa hết hạn đó (và khôi phục `context.User` trong `finally`). Request thiếu token hoặc token của user khác vẫn bị từ chối `400` như cũ.
4. **`tests/BranchFlow.Tests/Frontend/SessionTransportTests.cs`**:
   - Bổ sung unit test `ReloginAntiforgeryAcceptsTokenFromJustExpiredSessionButRejectsMissingOrDifferentUser` kiểm tra trực tiếp `WebAuthEndpoints.ValidateAntiforgeryAsync`.

---

## 4. Kết quả kiểm chứng (`18/11/2026`)

| Hạng mục kiểm chứng | Kết quả kỳ vọng (Expected) | Kết quả thực tế (Actual) | Trạng thái |
| :--- | :--- | :--- | :--- |
| Build `src/BranchFlow.Web` (`Release`) | `0 Warning(s), 0 Error(s)` | `0 Warning(s), 0 Error(s)` | **Pass** |
| Unit tests `tests/BranchFlow.Tests` (`Release`) | Tất cả unit test hiện có và test mới `SCRUM-140` đạt | `123 Passed, 0 Failed` (`SessionTransportTests`: `12/12 Passed`) | **Pass** |
| Kịch bản trình duyệt 5 bước `SCRUM-140` trên 1 origin sạch (`https://localhost:7133` + fault proxy trả `401` cho `POST /api/orders`) | Bước 1–3: giữ giỏ 1 ly `29.000đ`, hiện cảnh báo kết quả chưa xác định, khóa gửi lại, hiện nút *Đăng nhập lại*. Bước 4: nhấn *Đăng nhập lại* $\rightarrow$ mở `/login?notice=session` (`csrfLen=155`), nhấn *Đăng nhập* **1 lần** thành công ngay (vào `/employee`, không bị `/auth/login` `"Form đã hết hạn"`). Bước 5: quay lại `/orders/create` vẫn giữ nguyên giỏ 1 ly và cảnh báo. | Đạt trọn vẹn cả 5 bước ngay ở lần nhấn Đăng nhập đầu tiên (`tests/sprint5/SCRUM-140/Verification-2026-11-18.txt`) | **Pass** |
