# WBS-4.4.1 — Kết quả tự kiểm local

Người thao tác **từng dòng dưới đây: Công Thành, có công cụ hỗ trợ**; ngày thực hiện **10/11/2026**. Đây là tự kiểm bằng tài khoản bốn vai trò, không phải xác nhận của bốn người hoặc QA độc lập.

## Định danh lượt / phiên bản

- **L01**: lượt Chrome WebHTTPS17132/APIHTTPS17107, DBBranchFlow_Test, nền47541ee + working tree FE tại thời điểm kiểm. Các ảnh và actual được ghi trước khi lập gói này; không nhận vừa chạy lại toàn bộ UI. Các chức năng core và thay đổi local được ghi rõ trong nguồn/trích lịch sử. Không có SHA phát hành sạch của toàn bộ working tree trong bộ ảnh.
- **L02**: retest cùng ngày, nền47541ee + working tree, U25 trên Web17132; U27 qua Web17133/proxy401, U28 qua Web17134/proxytimeout. POST đã commit tại API17107 trước khi proxy đổi/chậm response. Đây là fault injection local tạo đơn, không phải mọi endpoint/kho.
- **B01**: phần chờ contract/quyền Owner đọc hồ sơ; nguồn API trả403 ngày10/11. Không tính Blocked là Pass.
- **S01**: đối chiếu SQL chỉ đọc khi lập gói này, log riêng. S01 không phát lại thao tác UI và không thay actual của người dùng.

Expected và điều kiện chấp nhận từng case ở [01-Kich-ban.md](01-Kich-ban.md); bảng dưới giữ cả expected/actual từ lượt ghi nhận. Cột “Minh chứng” là ảnh hỗ trợ và hồ sơ thực hiện; không phải ảnh nào cũng chụp đủ mọi bước của case. U04 dùng ảnh Owner để so tổng, chưa có ảnh Admin Tổng doanh thu riêng; U08/U14/U17 dựa thêm actual ghi nhậnL01, ảnh chỉ hỗ trợ ngữ cảnh.

| Mã / vai trò | Thao tác và kết quả cần đạt | Actual quan sát | Kết quả | Lượt | Minh chứng |
|---|---|---|---|---|---|
| U01 / Owner | Đăng nhập; xem tất cả chi nhánh, tổng đúng SQL | CN01 98.020,50đ, CN02 30.000đ; tổng 128.020,50đ, 4 đơn hoàn tất trước khi tạo đơn UAT | Pass | L01 | [Ảnh](evidence/owner-revenue-1366.png); L01/L02 record |
| U02 / Owner | Mở kho; xem tồn, cảnh báo, lịch sử; không có nút ghi | WBS5 CN01: 7 lít, ngưỡng 8, dưới ngưỡng; có IN/OUT, không có form ghi | Pass | L01 | [Ảnh](evidence/owner-stock-1366.png); L01/L02 record |
| U03 / Owner | Mở chi tiết đơn hủy mẫu | 0007: CANCELLED, 3 ly, topping 3+1, 147.000đ; không có sửa/hủy | Pass | L01 | [Ảnh](evidence/owner-cancelled-1366.png); L01/L02 record |
| U04 / Admin Tổng | Xem doanh thu toàn hệ thống | Ba lựa chọn chi nhánh, tổng cùng Owner | Pass | L01 | [Ảnh](evidence/owner-revenue-1366.png); L01/L02 record |
| U05 / Admin Tổng | Mở danh mục và form sửa món | Trà đào cam sả có M 30.000đ/L 38.000đ, form size/giá; không lưu đổi giá fixture | Pass cho mở form; lưu giá đã có kiểm chứng riêng ở mục 1 | L01 | [Ảnh](evidence/system-catalog-1366.png); L01/L02 record |
| U06 / Admin Tổng | Mở hồ sơ Employee A | Hiển thị đúng hồ sơ và nút lưu; ô phân công vừa vùng chứa sau sửa CSS | Pass cho mở form; không đổi tài khoản | L01 | [Ảnh](evidence/system-employees-768.png); L01/L02 record |
| U07 / Admin CN01 | Xem doanh thu đúng phạm vi | Chỉ có CN01; 98.020,50đ / 3 đơn hoàn tất trước và sau hủy đơn UAT | Pass | L01 | [Ảnh](evidence/branch-revenue-1366.png); L01/L02 record |
| U08 / Admin CN01 | Bật/tắt món tại chi nhánh, không sửa giá chung | FE-027436a6 mở bán có thông báo; ngừng bán lại thành công; không có form giá chung | Pass | L01 | [Ảnh](evidence/branch-menu-1366.png); L01/L02 record |
| U09 / Admin CN01 | Nhập 2 nguyên liệu FE-0d68e065 stock | Phiếu #18, tồn 7→9 g | Pass | L01 | [Ảnh](evidence/branch-stock-1366.png); L01/L02 record |
| U10 / Admin CN01 | Xuất 10 khi tồn 9 | Báo vượt tồn; SQL không có phiếu thất bại | Pass | L01 | [Ảnh](evidence/branch-stock-error-1366.png); L01/L02 record |
| U11 / Admin CN01 | Xuất hợp lệ 2 | Phiếu #19, tồn 9→7; mở lịch sử thấy trước/sau và -2 | Pass | L01 | [Ảnh](evidence/branch-stock-1366.png); L01/L02 record |
| U12 / Employee A | Đăng nhập, xem menu đúng CN01 | Có 11 món đang bán trên Test; món bị tắt tại đây không nằm trong menu bán | Pass | L01 | [Ảnh](evidence/employee-menu-360.png); L01/L02 record |
| U13 / Employee A | Tìm từ khóa không tồn tại rồi bỏ lọc | Có thông báo không tìm thấy món; bỏ lọc khôi phục danh sách | Pass | L01 | [Ảnh](evidence/employee-menu-empty-360.png); L01/L02 record |
| U14 / Employee A | Thêm món khi chưa chọn size | Báo phải chọn size, không thêm vào giỏ | Pass | L01 | [Ảnh](evidence/employee-options-1366.png); L01/L02 record |
| U15 / Employee A | M size, 3 Trân châu + 1 Thạch, 2 ly | Nút thêm Trân châu bị khóa ở 3 phần; tổng 98.000đ trong dialog/giỏ | Pass | L01 | [Ảnh](evidence/employee-cart-1366.png); L01/L02 record |
| U16 / Employee A | Xác nhận tạo đơn một lần | Máy chủ trả ID24/mã0009, 2 ly, 98.000đ, COMPLETED | Pass | L01 | [Ảnh](evidence/employee-created-1366.png); L01/L02 record |
| U17 / Employee A | Tra cứu mã không tồn tại; tra cứu mã0009 | Trạng thái rỗng có hướng dẫn; mã0009 trả đúng 1 đơn | Pass | L01 | [Ảnh](evidence/employee-history-1366.png); L01/L02 record |
| U18 / Employee A | Mở trực tiếp /orders/23, đơn của Employee B/CN02 | Báo không tìm thấy dữ liệu trong phạm vi, không lộ chi tiết | Pass | L01 | [Ảnh](evidence/employee-cross-branch-denied-1366.png); L01/L02 record |
| U19 / Admin CN01 | Đổi 2→3 ly nhưng bỏ trống lý do | Preview 147.000đ, lưu bị từ chối; chưa thêm lịch sử | Pass | L01 | [Ảnh](evidence/branch-adjust-reason-error-1366.png); L01/L02 record |
| U20 / Admin CN01 | Lưu 3 ly với lý do UAT | 147.000đ theo snapshot 49.000đ/ly; thêm một lịch sử | Pass | L01 | [Ảnh](evidence/branch-order-adjusted-1366.png); L01/L02 record |
| U21 / Admin CN01 | Hủy đơn bằng bước xác nhận và lý do UAT | CANCELLED; thêm lịch sử hủy; không còn nút sửa/hủy | Pass | L01 | [Ảnh](evidence/branch-order-cancelled-1366.png); L01/L02 record |
| U22 / Employee A | Mở lại /orders/24 | Thấy Đã hủy, 3 ly, 147.000đ; không có thao tác sửa/hủy | Pass | L01 | [Ảnh](evidence/employee-cancelled-1366.png); L01/L02 record |
| U23 / Admin CN01 | Tạm dừng đúng API Test rồi tải doanh thu | Có loading/khóa nút khi tải; sau lỗi có Thử lại; không hiện tổng 0 hoặc empty giả | Pass | L01 | [Ảnh](evidence/branch-revenue-offline-1366.png); L01/L02 record |
| U24 / Admin CN01 | Khởi động lại API Test, bấm Thử lại | Khôi phục 98.020,50đ / 3 đơn; lỗi biến mất | Pass | L01 | [Ảnh](evidence/branch-revenue-recovered-1366.png); L01/L02 record |
| U25 / Employee A | Nhấn xác nhận hai lần liên tiếp | Gửi clickCount=2; SQL từ10 lên11 đơn, chỉ có ID25/mã0010,29.000đ | Pass trong lượt đã chạy; không suy API idempotency | L02 | [Ảnh](evidence/retest-double-click.png); L01/L02 record |
| U26 / Owner | Đọc hồ sơ nhân viên chỉ xem | Đầu vào API/quyền Owner chưa được chốt ở mục 1 | Blocked; cần Thuần/Khang | B01 | Contract/API403; F01 |
| U27 / Employee A | POST đã commit, nhận401 rồi đăng nhập lại | ID26/mã0011,29.000đ có trong DB/lịch sử; giỏ1ly và cảnh báo còn sau login lại, gửi lại bị khóa | Pass trong lượt đã chạy | L02 | [Ảnh](evidence/retest-401-relogin-cart.png); SQL retest |
| U28 / Employee A | POST đã commit, client timeout | ID27/mã0012,29.000đ có trong DB/lịch sử; giữ giỏ/cảnh báo, khóa gửi lại | Pass trong lượt đã chạy | L02 | [Ảnh](evidence/retest-post-timeout.png); SQL retest |

## Tổng hợp và dữ liệu đối chiếu

**28 kịch bản local:27Pass/1Blocked (U26).** Không có kết quả public trong bảng local. Các mức Pass giới hạn đúng phần đã chạy, ví dụ mở form không đồng nghĩa tất cả CRUD đã được kiểm.

Đơn UI ID24/mã0009:2ly98.000→3ly147.000→CANCELLED, hai history thật. Kho #18 IN2 và #19 OUT2:7→9→7; xuất vượt tồn bị từ chối. Fixture tĩnh WBS5 tồn7/15/4, doanh thu riêng128.000. Tổng ảnh128.020,50/215.020,50 là snapshot cũ có giao dịch khác; không buộc tổng DB hiện tại bằng các số đó.

Minh chứng hỗ trợ: [trích actual gốcL01](evidence/records/Source-L01-Excerpt.md), [trích retestL02](evidence/records/Source-L02-Excerpt.md), [log SQL retest](evidence/records/Retest-PostFault-Sql.txt), [đối chiếu SQL mớiS01](evidence/records/Sql-Recheck-2026-11-10.txt). Chạy S01 bằng Verify-UatEvidence.sql tại cùng thư mục records; S01 chỉ đọc trạng thái giữ lại, không chứng minh lại các thao tác tương tác.

Đối chiếu sửa/hủy bổ sung đã bàn giao riêng SCRUM-124 tại commit4d14af2; không copy runner/commit code task khác vào nhánh4.4.1. Khi QA dùng candidate khác, chạy lượt mới và ghi SHA/actual mới; không đổi nhãn ảnh lịch sử thành bản mới.