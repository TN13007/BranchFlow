# Trích nguồnL01 lịch sử — không phải lượt mới

Nguồn: Sprint5-UAT-Responsive.md; SHA256: 73E4D6B188D3FC2A544A500B0C6B60A790137E73B00E03364D49BD495A061DFE
Candidate đã ghi:47541ee+working tree; Công Thành,10/11/2026. U25 NotRun trong nguồn cũ đã được thay bằng retestL02 ở bảng actual hiện hành.

| U01 / Owner | Đăng nhập; xem tất cả chi nhánh, tổng đúng SQL | CN01 98.020,50đ, CN02 30.000đ; tổng 128.020,50đ, 4 đơn hoàn tất trước khi tạo đơn UAT | Pass |
| U02 / Owner | Mở kho; xem tồn, cảnh báo, lịch sử; không có nút ghi | WBS5 CN01: 7 lít, ngưỡng 8, dưới ngưỡng; có IN/OUT, không có form ghi | Pass |
| U03 / Owner | Mở chi tiết đơn hủy mẫu | 0007: CANCELLED, 3 ly, topping 3+1, 147.000đ; không có sửa/hủy | Pass |
| U04 / Admin Tổng | Xem doanh thu toàn hệ thống | Ba lựa chọn chi nhánh, tổng cùng Owner | Pass |
| U05 / Admin Tổng | Mở danh mục và form sửa món | Trà đào cam sả có M 30.000đ/L 38.000đ, form size/giá; không lưu đổi giá fixture | Pass cho mở form; lưu giá đã có kiểm chứng riêng ở mục 1 |
| U06 / Admin Tổng | Mở hồ sơ Employee A | Hiển thị đúng hồ sơ và nút lưu; ô phân công vừa vùng chứa sau sửa CSS | Pass cho mở form; không đổi tài khoản |
| U07 / Admin CN01 | Xem doanh thu đúng phạm vi | Chỉ có CN01; 98.020,50đ / 3 đơn hoàn tất trước và sau hủy đơn UAT | Pass |
| U08 / Admin CN01 | Bật/tắt món tại chi nhánh, không sửa giá chung | FE-027436a6 mở bán có thông báo; ngừng bán lại thành công; không có form giá chung | Pass |
| U09 / Admin CN01 | Nhập 2 nguyên liệu FE-0d68e065 stock | Phiếu #18, tồn 7→9 g | Pass |
| U10 / Admin CN01 | Xuất 10 khi tồn 9 | Báo vượt tồn; SQL không có phiếu thất bại | Pass |
| U11 / Admin CN01 | Xuất hợp lệ 2 | Phiếu #19, tồn 9→7; mở lịch sử thấy trước/sau và -2 | Pass |
| U12 / Employee A | Đăng nhập, xem menu đúng CN01 | Có 11 món đang bán trên Test; món bị tắt tại đây không nằm trong menu bán | Pass |
| U13 / Employee A | Tìm từ khóa không tồn tại rồi bỏ lọc | Có thông báo không tìm thấy món; bỏ lọc khôi phục danh sách | Pass |
| U14 / Employee A | Thêm món khi chưa chọn size | Báo phải chọn size, không thêm vào giỏ | Pass |
| U15 / Employee A | M size, 3 Trân châu + 1 Thạch, 2 ly | Nút thêm Trân châu bị khóa ở 3 phần; tổng 98.000đ trong dialog/giỏ | Pass |
| U16 / Employee A | Xác nhận tạo đơn một lần | Máy chủ trả ID24/mã0009, 2 ly, 98.000đ, COMPLETED | Pass |
| U17 / Employee A | Tra cứu mã không tồn tại; tra cứu mã0009 | Trạng thái rỗng có hướng dẫn; mã0009 trả đúng 1 đơn | Pass |
| U18 / Employee A | Mở trực tiếp /orders/23, đơn của Employee B/CN02 | Báo không tìm thấy dữ liệu trong phạm vi, không lộ chi tiết | Pass |
| U19 / Admin CN01 | Đổi 2→3 ly nhưng bỏ trống lý do | Preview 147.000đ, lưu bị từ chối; chưa thêm lịch sử | Pass |
| U20 / Admin CN01 | Lưu 3 ly với lý do UAT | 147.000đ theo snapshot 49.000đ/ly; thêm một lịch sử | Pass |
| U21 / Admin CN01 | Hủy đơn bằng bước xác nhận và lý do UAT | CANCELLED; thêm lịch sử hủy; không còn nút sửa/hủy | Pass |
| U22 / Employee A | Mở lại /orders/24 | Thấy Đã hủy, 3 ly, 147.000đ; không có thao tác sửa/hủy | Pass |
| U23 / Admin CN01 | Tạm dừng đúng API Test rồi tải doanh thu | Có loading/khóa nút khi tải; sau lỗi có Thử lại; không hiện tổng 0 hoặc empty giả | Pass |
| U24 / Admin CN01 | Khởi động lại API Test, bấm Thử lại | Khôi phục 98.020,50đ / 3 đơn; lỗi biến mất | Pass |
| U25 / Employee A | Nhấn xác nhận hai lần liên tiếp | Đã chuẩn bị giỏ 1 ly 29.000đ; phiên trình duyệt đóng trước khi gửi. SQL vẫn 10 đơn, max ID24 | NotRun; không suy từ một lượt tạo thành công |
| U26 / Owner | Đọc hồ sơ nhân viên chỉ xem | Đầu vào API/quyền Owner chưa được chốt ở mục 1 | Blocked; cần Thuần/Khang |