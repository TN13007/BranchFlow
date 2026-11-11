# WBS-4.6.5 — Đánh giá tương thích Chrome/Edge/Android

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận đầu vào **WBS 4.6.2**, tổng hợp minh chứng và lập bàn giao ngày **11/11/2026**. Ngày 15/11 trong lịch ProjectLibre là kế hoạch trước cập nhật; hạn hoàn thành nội bộ chung là 13/11/2026. Jira tham chiếu: **SCRUM-127** (Story **SCRUM-107**). Nhánh riêng: `docs/SCRUM-127-wbs-4.6.5-browser-compatibility`. Người nhận tiếp: **Hòa Thuận — WBS 4.2.5** (2026-11-21) và **PM Khang** review/tích hợp.

## Đầu vào và phần em thực hiện trong WBS 4.6.5

Em tạo nhánh `docs/SCRUM-127-wbs-4.6.5-browser-compatibility` trực tiếp từ commit đầu vào **fe9a0e4f10edee55f88aaa2ca87c3b5b7b1f7d84** của **WBS 4.6.2** (`test/SCRUM-127-wbs-4.6.2-responsive`, đã gộp `origin/main` **ae595fd** cùng các bản sửa CSS `workspace-navigation.css`, `business.css`, `order-menu.css`). Tại thời điểm bàn giao, đầu vào `fe9a0e4` đang chờ Khang review/merge vào `origin/main`.

Vì toàn bộ thay đổi CSS và các phép kiểm Chrome/Edge ở 3 kích thước `360/768/1366px` đã có trong đầu vào `fe9a0e4`, đồng thời các lượt kiểm tương thích đọc/ghi trên Microsoft Edge và Chrome đã có bằng chứng thực tế ngày **10/11/2026** và **11/11/2026**, em không sửa thêm mã nguồn ứng dụng, không ghi thêm dữ liệu vào DB và không chạy lặp lại build/test để tính công trùng. Đầu ra mới của WBS 4.6.5 gồm:

- Tài liệu đánh giá tương thích trình duyệt/thiết bị `docs/WBS-4.6.5-Handoff.md` và chỉ mục provenance `docs/WBS-4.6.5-Evidence.json`.
- Trích lục log kiểm tra đọc/ghi trên Microsoft Edge và SQL đối chiếu ngày `10/11/2026` tại `tests/sprint5/WBS-4.6.5/Historical-Edge-Write-Excerpt.txt`.
- Bộ 3 ảnh Microsoft Edge lịch sử ngày `10/11/2026` (`edge-owner-revenue.png`, `edge-owner-stock.png`, `retest-edge-write.png`) được sao chép nguyên byte (khớp SHA256) vào `docs/evidence/WBS-4.6.5/` để đóng gói độc lập cùng bộ ảnh `WBS-4.6.2`.

## Môi trường và nguồn dữ liệu đối chiếu

1. **Lượt chạy ngày 11/11/2026 (từ đầu vào WBS 4.6.2 — commit `fe9a0e4`)**:
   - Windows, .NET 10 Release (`Build-API-2026-11-11.txt` và `Build-Web-2026-11-11.txt`: `0 Warning(s), 0 Error(s)`).
   - Web `https://localhost:17532` → API `https://localhost:17507` → SQL Server `localhost/BranchFlow_Test`.
   - **Google Chrome trên Windows** (viewport emulation `360×800`, `768×800`, `1366×800`): 24 bản ghi đo DOM (`Chrome-Measurements.json`) và 25 ảnh chụp màn hình.
   - **Microsoft Edge trên Windows** (DevTools Responsive emulation tại `Width = 360`, `768`, `1366`): kiểm màn Owner (`/owner`) và đăng xuất (`edge-owner-360/768/1366.png`, `Edge-360/768/1366-AX.txt`).
2. **Lượt chạy ngày 10/11/2026 (dùng lại có ghi rõ nguồn — nền `47541ee` + working tree `thanhnc`)**:
   - Web `https://localhost:17132` (và các host test proxy `17133`/`17134`) → API `https://localhost:17107` → SQL Server `localhost/BranchFlow_Test`.
   - **Google Chrome Windows (156.0.8078.81)**: các ca UAT 4 vai trò, chống gửi lặp `U25` (`clickCount=2` trên nút xác nhận giỏ hàng chỉ tạo 1 đơn `ID 25`), xử lý 401/timeout sau commit đơn (`ID 26`, `ID 27`), tạo/lưu danh mục `RETEST5` và giá lẻ `Topping 4,25đ`.
   - **Microsoft Edge Windows (157.0.4341.42)**:
     - Luồng chỉ đọc của Owner trên cửa sổ desktop: xem doanh thu (`edge-owner-revenue.png`) và tồn kho (`edge-owner-stock.png`).
     - Luồng ghi của Admin Tổng (`retest-edge-write.png` + `Retest-Catalog-Sql.txt`): đăng nhập trên Edge, lọc `RETEST5`, mở sửa món `RETEST5 Product`, nhập giá lẻ `15,75đ` bằng phím, lưu thành công và đối chiếu SQL (`15.75`); đăng xuất sạch phiên, không lưu mật khẩu trình duyệt.

## Ma trận đánh giá tương thích Chrome / Edge / Android

| Môi trường / Thiết bị | Chế độ kiểm tra | Phạm vi và kết quả thực tế (Expected → Actual) | Kết luận & Nguồn |
|---|---|---|---|
| **Google Chrome (Windows)** | Desktop `1366×800` & Viewport emulation `768×800`, `360×800` | - **Bố cục 3 kích thước (11/11)**: Sau khi sửa `order-menu.css`, `business.css` và `workspace-navigation.css`, cả 24 trạng thái đo DOM đều khớp vùng chứa (menu 360px giảm từ `399,75px` xuống `344,8px`, dialog tùy chọn/giỏ hàng vừa màn 360px, sidebar cố định khi cuộn ở 768px, form quản trị không tràn).<br>- **Trạng thái tải/rỗng/lỗi & chống gửi lặp (10/11 & 11/11)**: Tìm món rỗng, doanh thu rỗng, lỗi khoảng ngày hiển thị đúng; `U25` double-click xác nhận giỏ hàng chỉ tạo 1 đơn `ID 25` (`10 → 11`); 401/timeout sau commit giữ giỏ và bật cảnh báo `Cart.OutcomeUnknown`. | **Pass** trong phạm vi đã chạy (11/11 từ `WBS-4.6.2` + dùng lại 10/11) |
| **Microsoft Edge (Windows) — Luồng đọc & Responsive** | DevTools Responsive emulation `360`, `768`, `1366px` (11/11) & Desktop (10/11) | - **Responsive 3 kích thước (11/11)**: Màn Owner (`/owner`) ở `360×741`, `768×741`, `1366×800` hiển thị đúng tiêu đề, bộ lọc chi nhánh/ngày, nhãn `Chỉ xem dữ liệu`, thông báo trống/dữ liệu và đăng xuất về `/login?notice=logout` (`edge-owner-360/768/1366.png`, `Edge-*-AX.txt`).<br>- **Desktop chỉ đọc (10/11)**: Owner xem doanh thu và tồn kho CN01 (`edge-owner-revenue.png`, `edge-owner-stock.png`). | **Pass** cho phạm vi đã chạy (11/11 + dùng lại 10/11) |
| **Microsoft Edge (Windows) — Luồng ghi dữ liệu** | Desktop cửa sổ chuẩn (10/11) | - Admin Tổng đăng nhập trên Edge, lọc `RETEST5`, mở sửa món `RETEST5 Product`, nhập giá lẻ `15,75đ` bằng phím, blur và lưu; UI báo thành công (`retest-edge-write.png`) và SQL `BranchFlow_Test` ghi nhận `RETEST5 Product = 15.75` (`Historical-Edge-Write-Excerpt.txt`).<br>- Không nhận đã chạy lại toàn bộ biến thể ghi khác (tạo đơn/ghi kho) trên Edge. | **Pass** cho luồng cập nhật món đã chạy ngày 10/11; các luồng ghi khác trên Edge: **NotRun** |
| **Android (Thiết bị thật / Android Emulator)** | Không có thiết bị vật lý hoặc emulator Android kết nối | - Chỉ có kết quả viewport emulation `360px` trên Chrome và Edge Windows.<br>- Không gán viewport emulation trên máy tính thành kiểm thử trên trình duyệt/WebView Android thật. | **NotRun** (ghi rõ giới hạn môi trường) |
| **Môi trường Public HTTPS & QA độc lập** | Chờ URL/SHA triển khai public và QA retest | - Toàn bộ kết quả trên thực hiện tại local (`localhost` + `BranchFlow_Test`).<br>- Chưa có URL/SHA public và lượt kiểm độc lập của Hòa Thuận (`WBS 4.2.5`). | **NotRun** |

## Lưu ý kỹ thuật khi kiểm tra tương thích trình duyệt

- **Định dạng ô nhập ngày (`<input type="date">`) và số lẻ**: Ô chọn ngày dùng control native của Chromium (Chrome/Edge trên Windows hiển thị theo locale máy `MM/DD/YYYY`, trong khi bảng kết quả định dạng `dd/MM/yyyy` và payload API truyền chuẩn ISO `yyyy-MM-dd`). Khi nhập giá lẻ bằng công cụ tự động trên Blazor Server, cần gõ phím và blur trường để sự kiện bind cập nhật trước khi bấm Lưu.
- **Giới hạn dùng lại ảnh lịch sử (10/11/2026)**: Ảnh `edge-owner-revenue.png` ngày 10/11 chụp lúc đơn UAT đang `COMPLETED` (tổng `226.020,50đ`), sau khi hủy đơn trong kịch bản UAT thì tổng quay về `128.020,50đ`, và sau đợt retest 3 đơn mới là `215.020,50đ`. Các ảnh lịch sử giữ nguyên thời điểm chụp ngày 10/11 trên nền `47541ee` + working tree, không gán thành ảnh chụp mới của ngày 11/11.

## Minh chứng đóng gói

- Tài liệu và chỉ mục mới:
  - `docs/WBS-4.6.5-Handoff.md`
  - `docs/WBS-4.6.5-Evidence.json`
  - `tests/sprint5/WBS-4.6.5/Historical-Edge-Write-Excerpt.txt`
  - `docs/evidence/WBS-4.6.5/edge-owner-revenue.png`, `edge-owner-stock.png`, `retest-edge-write.png` (bản sao nguyên byte từ ngày 10/11, khớp SHA256).
- Minh chứng kế thừa trực tiếp từ commit đầu vào `fe9a0e4` (`WBS 4.6.2`) trên cùng nhánh:
  - `docs/WBS-4.6.2-Handoff.md`, `docs/WBS-4.6.2-Evidence.json`
  - `tests/sprint5/WBS-4.6.2/Chrome-Measurements.json`, `Edge-360-AX.txt`, `Edge-768-AX.txt`, `Edge-1366-AX.txt`, `Historical-DoubleClick-Excerpt.txt`, `Build-API-2026-11-11.txt`, `Build-Web-2026-11-11.txt`, `Provenance.txt`
  - 28 ảnh responsive Chrome/Edge ngày `11/11/2026` trong `docs/evidence/WBS-4.6.2/`.

## Bàn giao

- **Hòa Thuận (`WBS 4.2.5`)**: nhận ma trận tương thích Chrome/Edge/Android, các điểm lưu ý input date/blur số lẻ, bộ ảnh/log hai đợt (10/11 và 11/11) và danh sách mục `NotRun` (Android thật/emulator, các biến thể ghi còn lại trên Edge, smoke public) để đối chiếu khi thực hiện QA tích hợp.
- **PM Khang**: nhận nhánh `docs/SCRUM-127-wbs-4.6.5-browser-compatibility` (bao gồm cả đầu vào `WBS 4.6.2`) để review và tích hợp.
- Không tự chuyển Done cho toàn bộ **SCRUM-127** hoặc Story **SCRUM-107**; không sao chép estimate 2h thành actual.
