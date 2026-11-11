# WBS-3.3.1.5 — Tạo giao diện quản lý menu tổng

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, bổ sung kiểm chứng và bàn giao **11/11/2026**. Ngày 18/11 trong lịch là kế hoạch. Jira đúng WBS: **SCRUM-123**, Story **SCRUM-102**. Nhánh riêng: `feat/SCRUM-123-wbs-3.3.1.5-system-menu-ui` tách từ `origin/main` (**ae595fd**).

## Phần dùng lại và phần em đưa vào nhánh

Core trên `main` đã có schema/model `Category`, `Product`, `ProductSize`, `ToppingGroup`, `Topping`, `NoteGroup`, `NoteOption`, `BranchProduct`, `BranchTopping`, API quản trị trong `ManagementController` (`GET/POST/PUT /api/management/options/{kind}`, `products`, `toppings`, `notes`, `PUT /api/management/branches/{branchId}/availability/{kind}/{id}`), logic kiểm tra trong `MvpService` (chỉ `SYSTEM_ADMIN` tạo/sửa danh mục/món/size/giá chung/topping/ghi chú; giá không âm; tên trùng trả `409 Conflict`; `BRANCH_ADMIN` chỉ bật/tắt `IsAvailable` tại chi nhánh mình và bị chặn `403` khi gọi API sửa giá chung; không xóa phá lịch sử đơn và snapshot giá cũ) cùng component `ManagementMenu.razor`. Em giữ nguyên backend/schema/contract, tiếp nhận đầu vào API `3.3.1.4` của Trường Tài.

Em tiếp nhận nguyên vẹn file **`src/BranchFlow.Web/Components/Business/ManagementMenu.razor`** từ working tree `D:\QLDA\BranchFlow` (trùng khớp byte-for-byte SHA256 `2DCAD4900AC00764CC138031651100CDD54AB94416334F9DAA4AF25963DF8F1E`, Git blob `58c9e8fc4db9ea33cd67d881d753950ee4305629`):
- Bổ sung `OnRetry="ReloadAsync"` và `Busy="busy"` cho `ApiError`; xóa `notice` cũ khi thao tác lưu hoặc tải lại gặp lỗi để không hiển thị thông báo thành công giả.
- Khóa vùng thao tác `<fieldset disabled="@(busy || !loaded || needsLogin || accessDenied)">` khi đang tải/lưu, chưa có dữ liệu danh mục, mất phiên (`401`) hoặc bị từ chối quyền (`403`).
- Phân biệt rõ trạng thái đang tải (`Đang tải hoặc xử lý danh mục…`) và các trạng thái rỗng sau khi tải xong (`Chưa có món trong danh mục.`, `Không có món phù hợp với từ khóa.`, `Chưa có topping.`, `Chưa có lựa chọn ghi chú.`, `Danh mục chưa có dữ liệu.`).
- Dùng `BranchFlow.Web.Components.Orders.Display.Money(...)` để hiển thị giá món theo size và giá topping có phần thập phân theo chuẩn `vi-VN` (ví dụ `10,25đ`, `15,75đ`, `4,25đ`) thay vì làm tròn mất phần lẻ `N0`.

## Môi trường / phiên bản

- **Lượt kiểm chứng mới ngày 11/11/2026** trên nhánh `feat/SCRUM-123-wbs-3.3.1.5-system-menu-ui` (nền `origin/main` `ae595fd`):
  - Build Release `dotnet build src/BranchFlow.Web -c Release --nologo`: **0 Warning(s), 0 Error(s)** (`tests/sprint5/WBS-3.3.1.5/Build-Web-2026-11-11.txt`).
  - Unit test menu & định giá (`OrderMenuServiceTests`, `OrderPricingTests`, `CartTests`): **21/21 Pass, 0 Fail, 0 Skip** (`tests/sprint5/WBS-3.3.1.5/Unit-MenuPricing-2026-11-11.txt`).
  - Đối chiếu kết quả regression `tests/ManagementSmoke` trên `main` (`tests/sprint5/QA-Retest-2026-11-11.md`): xác nhận `BranchAdmin cannot create or change product prices` (`403`), `BranchAdmin cannot update a product or its prices` (`403`) và chữ ký SHA2-256 của toàn bộ bảng `Product`/`ProductSize` trong SQL không đổi.
- **Minh chứng HTTP, SQL & Browser lịch sử ngày 10/11/2026** (Web HTTPS `localhost:17132` → API HTTPS `localhost:17107` → SQL `BranchFlow_Test`, nền `47541ee` + working tree `thanhnc`):
  - Trích lục tại `tests/sprint5/WBS-3.3.1.5/Historical-HTTP-UI-Excerpt.txt` và 10 ảnh PNG nguyên bản tại `docs/evidence/WBS-3.3.1.5/`.

## Case / expected / actual

| Ca kiểm chứng | Expected | Actual / Nguồn | Kết quả |
|---|---|---|---|
| System Admin tạo danh mục, size, nhóm topping, nhóm ghi chú, món/size và topping qua UI | Tạo và lưu thành công vào `BranchFlow_Test`, SQL khớp quan hệ FK | Tạo bộ `RETEST5 Category`, `RETEST5 Size`, `RETEST5 ToppingGroup`, `RETEST5 NoteGroup`, `RETEST5 Note`, `RETEST5 Topping` (`4,25đ`), `RETEST5 Product` (`15,75đ`); ảnh `retest-catalog-notes.png`, `retest-catalog-product.png`, `retest-topping-price.png`, `retest-edge-write.png` & `Retest-Catalog-Sql.txt` | **Pass** (dùng lại 10/11) |
| Thêm size / sửa giá chung và hiển thị số tiền lẻ (`vi-VN`) | Giá mới phản ánh trên danh mục và menu bán; đơn cũ đã chốt giữ nguyên snapshot giá | HTTP tạo món giá `10.25`, sửa lên `12.50`; đơn cũ giữ `10.25` (`Verify-FrontendApis.ps1` dòng 47-77); UI hiển thị đúng phần lẻ (`menu-decimal.png`, `retest-topping-price.png`, `retest-edge-write.png`) | **Pass** (dùng lại 10/11 + unit 11/11) |
| Bật/tắt bán món và phản ánh trên menu Employee sau tải lại | Món ngừng bán không xuất hiện trong `GET /api/order-menu` của Employee | `U08` bật/tắt món `FE-027436a6` tại CN01 (`branch-menu-1366/768.png`); `U12` Employee A tải menu chỉ thấy các món đang mở bán; unit test `OrderMenuServiceTests` Pass | **Pass** (dùng lại 10/11 + unit 11/11) |
| Kiểm tra giá không hợp lệ (giá âm) và tên trùng | Giá âm bị chặn `400 Bad Request`; tên trùng bị chặn `409 Conflict` | `Verify-FrontendApis.ps1` dòng 79-80 (`PUT` giá `-1` trả `400`); `QA-Retest-2026-11-11.md` xác nhận UI từ chối giá âm; `ManagementController` bắt `SqlException 2601/2627` trả `409` | **Pass** (HTTP/backend; UI duplicate name chờ QA 3.3.1.6 retest thêm) |
| Quyền Admin Chi nhánh không được sửa giá chung / tạo món chung | UI chế độ `SystemMenu=false` ẩn toàn bộ form tạo/sửa món và giá chung, chỉ hiện nút `Mở bán`/`Ngừng bán`; gọi trực tiếp `POST/PUT /api/management/products` trả `403 Forbidden` | `ManagementMenu.razor` (`@if (SystemMenu)`), ảnh `branch-menu-1366/768.png`, `Verify-FrontendApis.ps1` dòng 78 (`403`), `ManagementApiSmoke.ps1` (`403`, SQL catalog signature unchanged) | **Pass** |
| Trạng thái loading / empty / error và tìm kiếm từ khóa rỗng | Hiện `Đang tải hoặc xử lý danh mục…`, báo rõ khi từ khóa không khớp, có nút `Thử lại` khi lỗi | `ManagementMenu.razor` (`loaded`, `OnRetry="ReloadAsync"`, thông báo rỗng theo `mode`) | **Pass** |

## Bàn giao và điều kiện chấp nhận còn lại

- **Người nhận tiếp theo**:
  - **Trường Tài (`3.3.2.2`)**: đối chiếu giao diện bật/tắt bán theo chi nhánh và contract menu.
  - **Hòa Thuận (`3.3.1.6`)**: thực hiện QA retest độc lập toàn bộ biến thể form danh mục/size/topping và ca nhập trùng tên trên UI.
  - **PM Khang**: đối chiếu WBS và review PR.
- **Chưa chạy (`NotRun`)**: Kiểm thử UI trùng tên danh mục/món trực tiếp trên trình duyệt của nhánh vừa tách, thiết bị Android thật, smoke public HTTPS và QA retest độc lập (`3.3.1.6`). Không tự chuyển trạng thái `SCRUM-123` hoặc Story `SCRUM-102` sang `Done`.
