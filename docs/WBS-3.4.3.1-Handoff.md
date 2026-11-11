# WBS-3.4.3.1 — Tạo màn hình lịch sử đơn do Employee lập

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, kiểm chứng và bàn giao **11/11/2026**. Ngày 17/11 trong lịch là kế hoạch. Jira tham chiếu: **SCRUM-124** (Story **SCRUM-103**). Nhánh riêng: `docs/SCRUM-124-wbs-3.4.3.1-employee-history` tách từ `origin/main` (**ae595fd**).

## Phần tiếp nhận trên core và phạm vi bàn giao

Theo đúng hướng dẫn WBS 3.4.3.1 (*“Tận dụng `/api/orders/mine` và trang lịch sử hiện có; chỉ bổ sung phần thiếu; nếu đầu ra đã hoàn thành thì kiểm chứng/handoff thay vì code lại”*), em rà soát và xác nhận các thành phần đã hoàn thiện sẵn trên `origin/main` (`ae595fd`):
- **Trang danh sách lịch sử đơn Employee** (`src/BranchFlow.Web/Components/Pages/Orders/Mine.razor`, blob `b26fd03b7c010c3fdf550856351bf43bc9a8e377`):
  - Route `/orders/mine`, giới hạn `[Authorize(Roles = "EMPLOYEE")]`, gọi API thật `Api.MineAsync(keyword.Trim(), businessDate)` (`GET /api/orders/mine`), không dùng dữ liệu preview.
  - Hỗ trợ lọc theo **Mã đơn hàng** (`keyword`) và **Ngày kinh doanh** (`businessDate`), nút **Tìm đơn** và **Bỏ bộ lọc**; xóa kết quả cũ trước khi tải mới để không hiển thị dữ liệu cũ nếu request lỗi.
  - Hiển thị đầy đủ các trạng thái: đang tải (`loading`), lỗi kết nối/mất phiên kèm nút thử lại (`ApiError OnRetry="LoadAsync"`), trạng thái rỗng (`Chưa có đơn hàng phù hợp với bộ lọc`) và danh sách kết quả (mã đơn, giờ VN, tổng số ly, tổng tiền snapshot, `StatusBadge`).
- **Trang chi tiết đơn Employee** (`src/BranchFlow.Web/Components/Pages/Orders/Detail.razor`, blob `211926707e2c32da2d23f0b296cd55beb703dcc6`):
  - Route `/orders/{Id:int}`, giới hạn `[Authorize(Roles = "EMPLOYEE")]`, gọi API thật `Api.DetailAsync(Id)` (`GET /api/orders/{id}`).
  - Hiển thị snapshot tại thời điểm xác nhận/cập nhật: chi nhánh, mã đơn, thời điểm tạo (giờ VN), ngày kinh doanh, trạng thái (`COMPLETED` / `CANCELLED`), tổng tiền, danh sách món/size/số ly/đơn giá snapshot, topping và ghi chú.
  - **Không có nút sửa số ly, không có nút hủy đơn tự do và giữ luồng báo sai nhiều bước trong phần hoãn** đúng phạm vi MVP.
- **Kiểm soát phạm vi trên Backend** (`OrdersController.cs`, `OrderService.cs`, `OrderRepository.cs`):
  - `GetMineAsync`: chỉ truy vấn đơn có `CreatedByUserId == userId` và `branchIds.Contains(o.BranchId)` (các chi nhánh Employee đang có phân công hiệu lực).
  - `GetDetailAsync`: kiểm tra `o.Id == id && o.CreatedByUserId == userId` đồng thời `access.CanAccessBranchAsync(userId, o.BranchId, ct)`; nếu Employee đổi URL `/orders/{id}` sang đơn của người khác hoặc chi nhánh khác, API trả `404` (`Không tìm thấy đơn trong phạm vi của bạn`).

Vì logic UI và API đã đáp ứng đầy đủ yêu cầu của WBS 3.4.3.1 trên `origin/main`, em không viết lại code ứng dụng mà đóng gói hồ sơ kiểm chứng, log build/unit mới ngày 11/11/2026 cùng bộ 9 ảnh chụp màn hình và trích lục HTTP/SQL lịch sử ngày 10/11/2026.

## Môi trường / phiên bản

- **Lượt kiểm chứng mới ngày 11/11/2026** trên nhánh `docs/SCRUM-124-wbs-3.4.3.1-employee-history` (`origin/main` `ae595fd`):
  - Build Release `dotnet build src/BranchFlow.Web -c Release --nologo`: **0 Warning(s), 0 Error(s)** (`tests/sprint5/WBS-3.4.3.1/Build-Web-2026-11-11.txt`).
  - Unit test `OrderServiceTests` & `SessionTransportTests`: **17/17 Pass, 0 Fail, 0 Skip** (`tests/sprint5/WBS-3.4.3.1/Unit-Orders-2026-11-11.txt`).
  - Đối chiếu script smoke `tests/sprint5/OrderLifecycleSmoke.ps1` đã hợp nhất trên `main`: xác nhận `GET /api/orders/{id}` trả `200` với trạng thái `CANCELLED` cho chính Employee A và trả `404` khi Employee B truy cập đơn của Employee A.
- **Minh chứng HTTP, SQL & Browser lịch sử ngày 10/11/2026** (Web HTTPS `localhost:17132` → API HTTPS `localhost:17107` → SQL `BranchFlow_Test`, nền `47541ee` + working tree `thanhnc`):
  - Trích lục tại `tests/sprint5/WBS-3.4.3.1/Historical-HTTP-UI-Excerpt.txt` và 9 ảnh PNG nguyên bản tại `docs/evidence/WBS-3.4.3.1/`.

## Case / expected / actual

| Ca kiểm chứng | Expected | Actual / Nguồn | Kết quả |
|---|---|---|---|
| Employee xem danh sách lịch sử đơn của mình (`/orders/mine`) và lọc theo mã / ngày kinh doanh | Gọi `/api/orders/mine`, lọc đúng theo `keyword` và `businessDate`, hiển thị ở 1366/768/360px | Lọc mã `0004` / `0009` và ngày `10/11/2026` trả đúng 1 đơn; ảnh `employee-history-1366.png`, `employee-history-768.png`, `employee-history-360.png` (U17) | **Pass** (dùng lại 10/11) |
| Trạng thái rỗng khi tra cứu mã không tồn tại | Không báo lỗi giả, hiển thị thông báo rỗng và hướng dẫn xóa bộ lọc | Ảnh `employee-history-empty-360.png` (`Chưa có đơn hàng phù hợp với bộ lọc`) (U17) | **Pass** (dùng lại 10/11) |
| Hiển thị chi tiết đơn và trạng thái sau khi Admin sửa/hủy (`/orders/{id}`) | Phản ánh đúng số ly đã sửa (3 ly, `147.000đ`) và trạng thái `Đã hủy` (`CANCELLED`); giữ nguyên snapshot giá khi Admin đổi giá chung | `Verify-FrontendApis.ps1` (`PASS Employee detail reflects admin cancellation`, `PASS Old order snapshot survives common price update`), `UatJourneySqlResult-2026-11-10.txt` (ID 24), ảnh `employee-cancelled-1366/768/360.png`, `employee-cancelled-detail.png` (U22) | **Pass** (dùng lại 10/11 + regression main 11/11) |
| Không có quyền tự sửa/hủy tự do hoặc báo sai nhiều bước | UI Employee không có nút sửa số ly/hủy đơn; gọi trực tiếp `PUT /api/management/orders/{id}` bằng token Employee trả `403 Forbidden` | `Detail.razor` chỉ có liên kết điều hướng; `Verify-FrontendApis.ps1` dòng 68 & `OrderLifecycleSmoke.ps1` dòng 77 (`403`) | **Pass** |
| Chặn đọc đơn của nhân viên khác / chi nhánh khác khi đổi `id` trên URL | Trả `404`, hiển thị lỗi không tìm thấy đơn trong phạm vi, không lộ chi tiết món/tiền của đơn người khác | Mở `/orders/23` (đơn của Employee B / CN02) hiển thị `Không tìm thấy đơn trong phạm vi của bạn` (`employee-cross-branch-denied-1366.png`, U18); `OrderLifecycleSmoke.ps1` dòng 99 (`404`) | **Pass** |
| Build Release Web & Unit test Order/Session | 0 warning/error; tất cả test trong phạm vi đạt | Build Web `0 Warning(s), 0 Error(s)`; `OrderServiceTests` + `SessionTransportTests` `17/17 Pass` ngày 11/11/2026 | **Pass** (11/11) |

## Tồn đọng và người nhận

- **Người nhận**: PM **Khang** đối chiếu WBS và review; QA **Hòa Thuận** thực hiện retest độc lập (kèm các biến thể lỗi mạng/500 trên trang lịch sử nếu cần).
- **Chưa chạy (`NotRun`)**: Fault-inject lỗi mạng trực tiếp trên trang `/orders/mine`, kiểm thử trên thiết bị Android thật, smoke trên URL public HTTPS và QA retest độc lập. Không tự đóng `SCRUM-124` hoặc Story `SCRUM-103`.
