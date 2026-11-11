# WBS-3.2.2.5 — Hiển thị hồ sơ nhân viên và kiểm quyền Owner chỉ đọc

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, bổ sung kiểm chứng và bàn giao **11/11/2026**. Ngày 16/11 trong lịch là kế hoạch. Jira đúng WBS: **SCRUM-122**, Story **SCRUM-101**. Nhánh riêng: `feat/SCRUM-122-wbs-3.2.2.5-employee-profiles` tách từ `origin/main` (**ae595fd**).

## Phần dùng lại và phần em đưa vào nhánh

Core trên `main` đã có schema/model `AppUser`, `EmployeeProfile`, `EmployeeBranchAssignment`, `AdminBranchAssignment`, `EmployeeStockPermission`, controller `ManagementController` (`GET/POST/PUT /api/management/users`, `POST /api/management/assignments`), service `MvpService` (kiểm quyền theo vai trò/chi nhánh, chặn phân công trùng đang hiệu lực 409, chặn khóa Branch Admin còn phân công hiệu lực 400, DTO `UserView` không lộ password/hash/token) và component `ManagementWorkspace.razor`. Em giữ nguyên backend/schema/contract, không mở thêm quyền API ngoài phạm vi FE, không xóa lịch sử đơn, không mở điều chuyển liên chi nhánh.

Em tiếp nhận nguyên vẹn file **`src/BranchFlow.Web/Components/Business/ManagementWorkspace.razor`** từ working tree `D:\QLDA\BranchFlow` (trùng khớp byte-for-byte SHA256 `904F8A7D8BB1CCAD059B5ED7E6435F7EC5590D737D028918F7DE1AF3E228298D`, Git blob `d1047067f929057a2f06dab9b9f5a85f6bc20982`):
- Giới hạn hiển thị nút và form **Sửa hồ sơ** theo vai trò (`CanEditUsers`, `CanEditUser`):
  - **System Admin** (`IsSystem`): có form tạo tài khoản mới, form phân công chi nhánh, nút sửa hồ sơ cho `BRANCH_ADMIN` và `EMPLOYEE`, và các checkbox khóa/mở tài khoản (`IsActive`) cùng quyền kho (`CanReceive`, `CanIssue`).
  - **Branch Admin**: chỉ hiện nút **Sửa hồ sơ** đối với tài khoản có vai trò `EMPLOYEE` trong danh sách thuộc phạm vi chi nhánh của mình; không hiện form tạo tài khoản, không hiện form phân công, không hiện checkbox khóa/mở hay đổi quyền kho.
  - **Owner**: `CanEditUsers = false`, `CanEditUser(_) = false` — trên UI không có bất kỳ nút tạo/sửa/phân công nhân viên nào; ngoài ra server vẫn chặn mọi lệnh ghi (`PUT /api/management/users/{id}`, `POST /api/management/users`, `POST /api/management/assignments`) với mã `403 Forbidden`.
- Bổ sung `OnRetry="ReloadAsync"` cho `ApiError`, khóa `<fieldset disabled="@(busy || needsLogin || accessDenied)">` khi đang gửi, mất phiên (`401`) hoặc bị từ chối truy cập (`403`), và chỉ hiển thị trạng thái rỗng/tổng sau khi tải dữ liệu thành công (`loaded`).

## Môi trường / phiên bản

- **Lượt kiểm chứng mới ngày 11/11/2026** trên nhánh `feat/SCRUM-122-wbs-3.2.2.5-employee-profiles` (nền `origin/main` `ae595fd`):
  - Build Release `dotnet build src/BranchFlow.Web -c Release --nologo`: **0 Warning(s), 0 Error(s)** (`tests/sprint5/WBS-3.2.2.5/Build-Web-2026-11-11.txt`).
  - Unit test quyền truy cập/khóa tài khoản/session (`MvpAdminDeactivationTests`, `BranchAccessServiceTests`, `AuthServiceTests`): **29/29 Pass, 0 Fail, 0 Skip** (`tests/sprint5/WBS-3.2.2.5/Unit-Access-2026-11-11.txt`).
  - Đối chiếu kết quả regression HTTP/SQL cô lập `tests/ManagementSmoke` đã hợp nhất trên `main` (`tests/sprint5/QA-Retest-2026-11-11.md`): **72/72 assertions Pass** trên `BranchFlow_Test`.
- **Minh chứng HTTP & UI lịch sử ngày 10/11/2026** (nền `47541ee` + working tree `thanhnc`, Web HTTPS `localhost:17132` → API HTTPS `localhost:17107` → SQL `BranchFlow_Test`):
  - Trích lục có kiểm soát tại `tests/sprint5/WBS-3.2.2.5/Historical-HTTP-Excerpt.txt` và 3 ảnh chụp nguyên bản không chỉnh sửa tại `docs/evidence/WBS-3.2.2.5/`.

## Case / expected / actual

| Ca kiểm chứng | Expected | Actual / Nguồn | Kết quả |
|---|---|---|---|
| System Admin tạo tài khoản Employee kèm phân công đầu và khóa/mở tài khoản | Tạo thành công, khóa (`IsActive=false`) và mở lại (`IsActive=true`) qua `POST/PUT /api/management/users` | `Verify-FrontendApis.ps1` (`PASS System Admin can create and lock fixture employee`) & `ManagementApiSmoke.ps1` | **Pass** (dùng lại 10/11 + regression main 11/11) |
| Chặn khóa Branch Admin đang còn phân công hiệu lực | Từ chối `400 Bad Request`, giữ nguyên `IsActive=true` và phân công trong SQL | Unit test `MvpAdminDeactivationTests` (2/2 Pass ngày 11/11) & `ManagementApiSmoke.ps1` | **Pass** (11/11) |
| Ràng buộc chỉ một phân công đang hiệu lực (3.2.2.4 → 3.2.2.5) | Phân công lần hai cho Employee đang có phân công hiệu lực hoặc Branch Admin trùng chi nhánh bị chặn `409 Conflict` | `POST /api/management/assignments` trả `409`; SQL giữ đúng 1 phân công hiệu lực (`Verify-FrontendApis.ps1`, `ManagementApiSmoke.ps1`) | **Pass** (dùng lại 10/11 + regression main 11/11) |
| Branch Admin chỉ sửa hồ sơ Employee thuộc quyền; chặn leo thang quyền/đổi chi nhánh | UI chỉ hiện nút `Sửa hồ sơ` cho `EMPLOYEE`; API chỉ cập nhật thông tin hồ sơ (`FullName`, `Email`, `Phone`, `Position`), giữ nguyên `Role`, `IsActive`, `BranchId`, quyền kho | `ManagementWorkspace.razor` (`CanEditUser`) + `ManagementApiSmoke.ps1` (`BranchAdmin profile update preserves protected SQL fields`) | **Pass** |
| Branch Admin sửa hồ sơ nhân viên ngoài chi nhánh (cross-branch) | Không thấy nhân viên CN02 trong danh sách CN01; gọi thẳng `PUT /api/management/users/{id}` ngoài chi nhánh bị chặn `403 Forbidden`, SQL không đổi | `Verify-FrontendApis.ps1` dòng 103 (`403`) & `ManagementApiSmoke.ps1` dòng 364-369 (`403`, SQL unchanged) | **Pass** |
| DTO không lộ mật khẩu / hash / token | Phản hồi `GET /api/management/users` không chứa `password`, `passwordHash` hoặc token | `UserView` DTO + `ManagementApiSmoke.ps1` (`Management response omits password and hash fields`) | **Pass** |
| UI System Admin hiển thị danh sách nhân viên và form hồ sơ ở 1366px / 768px | Hiển thị bảng nhân viên, vai trò, chi nhánh, trạng thái, form sửa hồ sơ và ô chọn tài khoản phân công không tràn vùng chứa | Ảnh `system-employees-1366.png`, `system-employees-768.png` (U06 Pass mở form) | **Pass** (dùng lại 10/11) |
| Kiểm quyền Owner không được ghi hồ sơ/phân công | Trên UI không có nút tạo/sửa/phân công; gọi trực tiếp `PUT /api/management/users/{id}` hoặc `POST /api/management/assignments` bằng token Owner bị chặn `403 Forbidden` | `ManagementWorkspace.razor` (`CanEditUsers=false`), `Verify-FrontendApis.ps1` dòng 113 (`403`), ảnh `owner-stock-readonly.png` | **Pass** (phần chặn ghi) |
| Owner đọc danh sách/hồ sơ nhân viên chỉ xem (U26) | Owner xem được danh sách/hồ sơ nhân viên ở chế độ chỉ đọc | **BLOCKER**: Backend `GET /api/management/users` đang đặt `[Authorize(Roles = "SYSTEM_ADMIN,BRANCH_ADMIN")]`, trả `403` với Owner; menu Owner chưa mở tab nhân viên. Em không tự ý sửa quyền backend ngoài phạm vi FE | **Blocked** (chờ Phước Thuần 3.2.2.4 & PM Khang chốt quyền/API) |

## Phụ thuộc / Tồn đọng bàn giao

1. **Blocker cần Phước Thuần (`3.2.2.4`) và PM Khang chốt**: Quyền đọc danh sách/hồ sơ nhân viên cho vai trò **Owner** trên API (`GET /api/management/users` hoặc endpoint đọc riêng cho Owner) và phạm vi hiển thị trên thanh điều hướng Owner. Ngay khi backend chốt contract, FE sẽ mở tab đọc tương ứng và retest U26.
2. **Kiểm thử độc lập**: Hòa Thuận (QA) retest trên bản tích hợp cuối và PM Khang nghiệm thu. Không tự chuyển trạng thái Jira sang `Done`.
