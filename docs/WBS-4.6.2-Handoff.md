# WBS-4.6.2 — Kiểm thử responsive ở 360/768/1366px

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, bổ sung kiểm chứng và lập bàn giao ngày **11/11/2026**. Ngày 14/11 trong lịch ProjectLibre là kế hoạch trước cập nhật; hạn hoàn thành nội bộ chung là 13/11/2026. Jira tham chiếu: **SCRUM-127** (Story **SCRUM-107**). Nhánh riêng: `test/SCRUM-127-wbs-4.6.2-responsive`.

## Đầu vào, phần dùng lại và phần em đưa vào nhánh

Nhánh được tạo từ `origin/main` (**af77b9e**), nhận commit ứng viên **3dbd79e** (`src/BranchFlow.Web/wwwroot/css/workspace-navigation.css`: giữ `.employee-sidebar` cố định trong viewport từ `768px` trở lên và chỉ cuộn `.workspace-body`), sau đó merge `origin/main` mới nhất (**ae595fd**, gồm PR #51 `WBS-5.1.3` và PR #52 `WBS-3.5.1.5`) tại commit merge **c2e7619**. Đầu vào **WBS 3.6.2.3** (`1488e9e`) đã có sẵn trên `main`.

Em không viết lại các màn hình hoặc backend; chỉ đưa vào nhánh 2 chỉnh sửa CSS đúng phạm vi sửa lỗi tràn bố cục khi kiểm responsive:

1. `src/BranchFlow.Web/wwwroot/css/order-menu.css`: thêm `width: 100%; min-width: 0;` cho `.order-menu-workspace`, `min-width: 0;` cho `.order-menu-hero > div` và `flex-shrink: 0;` cho `.order-menu-cart-button`. Trước khi sửa, ở viewport `360×800`, `.order-menu-workspace` giữ bề rộng `399,75px` (`scrollWidth = 400px`) trong khi `.workspace-body` chỉ rộng `344,8px`, làm nút mở giỏ hàng và cột thẻ món bên phải bị tràn tới `right = 383,75px`.
2. `src/BranchFlow.Web/wwwroot/css/business.css`: thêm `.business-shell .surface-card :is(.form-control, .form-select) { min-width: 0; max-width: 100%; }` để các ô chọn tài khoản/chi nhánh/nguyên liệu có chuỗi dài co vừa hàng form trong thẻ quản trị ở `360px` và `768px`.

Các ca chống gửi lặp (`U25` double-click tạo đơn) và trạng thái loading/fault-inject đã có lượt chạy thật ngày **10/11/2026** trên `BranchFlow_Test` nên em trích dùng lại kết quả đúng phạm vi và đối chiếu source hiện hành, không chạy lại thao tác ghi đơn lặp để tránh tạo thêm dữ liệu rác trên DB Test.

## Môi trường và dữ liệu kiểm chứng

- **Lượt chạy mới (11/11/2026)**:
  - Hệ điều hành & SDK: Windows, .NET 10 Release (`dotnet build` đạt `0 Warning(s), 0 Error(s)` cho cả `BranchFlow.Api` và `BranchFlow.Web`).
  - Dịch vụ & DB: Web `https://localhost:17532` → API `https://localhost:17507` → SQL Server `localhost/BranchFlow_Test` (chỉ đọc và thao tác UI không ghi bẩn fixture dùng chung `BF-WBS5-v1`).
  - Google Chrome trên Windows (viewport emulation `360×800`, `768×800`, `1366×800`): thu thập tự động 24 bản ghi kích thước DOM (`tests/sprint5/WBS-4.6.2/Chrome-Measurements.json`) và 25 ảnh chụp màn hình (`docs/evidence/WBS-4.6.2/`).
  - Microsoft Edge trên Windows (DevTools Responsive emulation tại `Width = 360`, `768`, `1366`): kiểm tra màn Owner (`/owner`) và luồng đăng xuất, lưu 3 ảnh (`edge-owner-360.png`, `edge-owner-768.png`, `edge-owner-1366.png`) cùng 3 bản trích accessibility tree (`Edge-360-AX.txt`, `Edge-768-AX.txt`, `Edge-1366-AX.txt`).
- **Lượt dùng lại có ghi nguồn (10/11/2026)**:
  - Ca `U25` double-click xác nhận giỏ hàng và ca 401/timeout sau commit đơn từ `D:/QLDA/BranchFlow/docs/Sprint5-Retest-Completion.md` và `Retest-DoubleClick-Sql.txt` (trích tại `tests/sprint5/WBS-4.6.2/Historical-DoubleClick-Excerpt.txt`).
  - Ảnh trạng thái loading doanh thu `branchadmin-loading.png` từ `WBS-3.6.2.3` (`commit 1488e9e`).

## Kết quả kiểm chứng theo ca (Expected / Actual / Trạng thái)

| Ca kiểm chứng | Kích thước / Trình duyệt | Expected | Actual đã ghi nhận | Kết quả |
|---|---|---|---|---|
| Form đăng nhập & thông báo đăng xuất | 360 / 768 / 1366px (Chrome) | Form và nút đăng nhập nằm gọn trong viewport, không tràn ngang | `documentWidth` = 360 / 753 / 1351px; `outliers = 0`; ảnh `login-360/768/1366.png` rõ chữ | Pass 11/11 |
| Menu gọi món Employee — lỗi trước sửa | 360×800 (Chrome) | Phát hiện và ghi nhận baseline nếu tràn | `employee-menu-360`: `.order-menu-workspace` rộng `399,75px` (`scrollWidth = 400`), 13 phần tử tràn phải tới `383,75px`; ảnh `before-employee-menu-360.png` | Baseline lỗi trước sửa |
| Menu gọi món Employee — sau sửa CSS | 360 / 768 / 1366px (Chrome) | Khung menu, ô tìm kiếm, nút giỏ hàng và thẻ món vừa khít vùng nội dung | `retest-menu-360`: `menu.width = 344,8px`, `scrollWidth = 345px`; `retest-menu-768`: `menu.width = 482,8px`; `retest-menu-1366`: `menu.width = 1081,2px`. Thẻ món và nút giỏ không còn tràn (chỉ thanh chip danh mục cuộn ngang cục bộ theo thiết kế); ảnh `retest-menu-360/768/1366.png` | Pass 11/11 |
| Hộp thoại tùy chọn món (size/topping/ghi chú) | 360×800 (Chrome) | Dialog mở vừa màn hình nhỏ, đọc và chọn được tùy chọn | `options-360`: `dialog` tại `x = 9,9px`, `width = 325px`, `right = 334,9px` (< 360px); ảnh `options-360.png` | Pass 11/11 |
| Hộp thoại giỏ hàng Employee | 360 / 768 / 1366px (Chrome) | Giỏ hàng không tràn viewport, nút xác nhận/tăng giảm rõ ràng | `cart-360`: `dialog.width = 306,8px` (`right = 325,8px`); `cart-768`: `width = 714,8px`; `cart-1366`: `width = 800px`; ảnh `cart-360/768/1366.png` | Pass 11/11 |
| Cố định sidebar khi cuộn nội dung | 768×800 (Chrome) | Sidebar đứng yên trong viewport, chỉ `.workspace-body` cuộn dọc | `sidebar-scroll-768`: `.workspace-body` cuộn `scrollTop = 1600`, `pageY = 0`, `.employee-sidebar` giữ nguyên `x = 0, width = 270, height = 800, scrollTop = 0`; ảnh `sidebar-scroll-768.png` | Pass 11/11 |
| Quản trị nhân viên & phân công (Admin Tổng) | 360 / 768 / 1366px (Chrome) | Các ô input/select dài không tràn thẻ; bảng dữ liệu cuộn trong `.table-responsive` | `system-employees-360/768/1366`: `pane.scrollWidth == pane.clientWidth` (`345` / `483` / `1081px`), `outliers = 0`; ảnh `system-employees-360/768/1366.png` | Pass 11/11 |
| Trạng thái rỗng (Empty state) | 360px & 1366px (Chrome) | Hiển thị rõ thông báo rỗng, không vỡ khung | Tìm món không khớp ở 360px (`empty-menu-360.png`) và doanh thu ngày trống ở 1366px (`revenue-empty-1366.png`), không tràn | Pass 11/11 |
| Trạng thái lỗi (Error state) | 360px & 768px (Chrome) | Báo lỗi rõ khi khoảng ngày không hợp lệ (`from > to`), giữ nguyên bộ lọc để sửa lại | `revenue-error-360.png` và `revenue-error-768.png`: hiện cảnh báo lỗi, `outliers = 0` | Pass 11/11 |
| Hiển thị dữ liệu doanh thu | 360 / 768 / 1366px (Chrome) | Thẻ tổng hợp và bảng doanh thu hiển thị đúng, không tràn viewport | `revenue-data-360/768/1366`: `outliers = 0`, bảng cuộn cục bộ trong `.table-responsive`; ảnh `revenue-data-360/768/1366.png` | Pass 11/11 |
| Kiểm tra trên Microsoft Edge | 360 / 768 / 1366px (Edge Windows emulation) | Màn Owner hiển thị đúng bố cục, chỉ xem dữ liệu và đăng xuất sạch phiên | Đạt ở cả 3 kích thước trên Edge DevTools Responsive; ảnh `edge-owner-360/768/1366.png` và log `Edge-360/768/1366-AX.txt` | Pass 11/11 (emulation trên Windows) |
| Trạng thái đang tải (Loading) & chống gửi lặp | Source + kết quả 10/11 | Khóa nút khi đang xử lý (`Busy`/`submitting`), `clickCount=2` không tạo đơn trùng | Source `Create.razor`, `OrderWorkspace.razor`, `ManagementStock.razor` giữ nguyên guard; log `Historical-DoubleClick-Excerpt.txt` ghi `U25` `clickCount=2` chỉ tạo 1 đơn `ID 25` (`10 → 11`) | Pass đối chiếu source + dùng lại runtime 10/11 |
| Build dự án bị ảnh hưởng | .NET 10 Release | Build API và Web sau khi gộp `main` và sửa CSS không lỗi/cảnh báo | `Build-API-2026-11-11.txt` và `Build-Web-2026-11-11.txt`: `0 Warning(s), 0 Error(s)` | Pass 11/11 |
| Thiết bị Android thật / Android emulator | Android | Kiểm trên máy thật hoặc emulator Android nếu có | Không có thiết bị/emulator Android kết nối; không gán Chrome/Edge viewport 360px thành Android thật | NotRun |
| Smoke public HTTPS & QA độc lập | Public / QA | Kiểm trên URL public sau deploy và QA retest độc lập | Chờ URL/SHA public và Hòa Thuận (`WBS 4.2.5`) / Khang review | NotRun |

## Minh chứng và tái lập

- `tests/sprint5/WBS-4.6.2/Chrome-Measurements.json`: 24 bản ghi đo DOM tự động trên Chrome ở 3 kích thước `360`, `768`, `1366px`.
- `tests/sprint5/WBS-4.6.2/Edge-360-AX.txt`, `Edge-768-AX.txt`, `Edge-1366-AX.txt`: log accessibility tree trên Microsoft Edge ở 3 kích thước.
- `tests/sprint5/WBS-4.6.2/Build-API-2026-11-11.txt`, `Build-Web-2026-11-11.txt`, `Provenance.txt`: log build Release và thông tin commit/blob/DLL hash.
- `tests/sprint5/WBS-4.6.2/Historical-DoubleClick-Excerpt.txt`: trích lục kết quả chống gửi lặp `U25` ngày `10/11/2026`.
- `docs/evidence/WBS-4.6.2/`: 28 ảnh chụp màn hình thực tế (gồm ảnh `before-employee-menu-360.png` trước sửa và bộ ảnh sau sửa trên Chrome/Edge), đã xoá thông tin đăng nhập sau phiên.
- `docs/WBS-4.6.2-Evidence.json`: chỉ mục toàn bộ file, commit, Git blob và SHA256.

Lệnh kiểm tra build tại root nhánh:

```powershell
dotnet build src/BranchFlow.Api -c Release --nologo
dotnet build src/BranchFlow.Web -c Release --nologo
```

## Bàn giao và điểm còn chờ

- **Bàn giao tiếp**:
  - **WBS 4.6.5 (Công Thành)**: dùng bộ đo và kết quả Chrome/Edge Windows kèm trạng thái `NotRun` của thiết bị Android thật để tổng hợp đánh giá tương thích trình duyệt/thiết bị.
  - **WBS 4.2.5 (Hòa Thuận)** và **PM Khang**: nhận PR/commit, bộ 28 ảnh, file đo DOM và checklist để review/retest tích hợp.
- **Giới hạn trung thực**:
  - Các phép kiểm `360px` và `768px` trên Chrome/Edge được thực hiện bằng **viewport emulation trên Windows**, không phải thiết bị Android hay tablet vật lý.
  - Ca double-click tạo đơn dùng lại kết quả SQL/browser ngày `10/11/2026` và đối chiếu source vì logic `Create.razor` / `OrderWorkspace.razor` không đổi; chưa phát sinh lượt ghi đơn lặp mới ngày `11/11/2026`.
  - Không tự chuyển trạng thái Done cho toàn bộ **SCRUM-127** hoặc Story **SCRUM-107**; không sao chép estimate 2h thành actual.
