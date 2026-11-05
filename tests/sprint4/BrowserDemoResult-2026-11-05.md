# Browser / SQL - bản tích hợp PR 26, 27

Ngày: 05/11/2026. Người thực thi: agent hỗ trợ Khang theo yêu cầu PM; không ghi đây là review độc lập của Thuận. Môi trường: Edge, Web HTTPS localhost:17132, API HTTPS localhost:17107, BranchFlow_Dev local. Không dùng mock API hoặc nút preview tạo đơn giả.

| Thao tác thực thi | Kết quả |
|---|---|
| Đăng nhập Employee A | Đúng chi nhánh BF-DEMO-01, đọc menu SQL; sau PR 26 có thêm menu demo của Tài |
| 2 ly, size 30.000, topping 3×5.000 + 4.000 | Giỏ hiển thị 98.000đ; source browser không gửi giá |
| Xác nhận trên giao diện Thành PR 27 | Chuyển `/orders/2`, mã BF-DEMO-01-20261105-0002, 98.000đ, COMPLETED/Hoàn tất |
| Truy vấn SQL cùng mã | Header, số ly 2, subtotal 98.000 và chi tiết khớp; xem order-sql.txt |
| Lịch sử đơn | Menu điều hướng và danh sách mở được; chứa mã đơn thật |
| Kiểm tra responsive | Chụp chi tiết tại 390×844 sau tích hợp; ảnh order-mobile.png |
| Logout trên bản tích hợp | Cookie Web kết thúc; mở orders/create chuyển login với ReturnUrl |
| Owner, System Admin, Branch Admin | Đăng nhập/đăng xuất trên UI mới, mở account đúng người; ảnh theo username |
| Employee B xem đơn ID 2 của A | Giao diện báo không tìm thấy trong phạm vi; không hiển thị số tiền/chi tiết đơn A |
| Retest SCRUM-108 sau sửa | Thêm 2 ly, mở giỏ ngay, sửa 1 ly: 49.000đ; giảm Trân châu 3→0: 34.000đ; xóa dòng: 0đ. Giữ animation, khóa thao tác trong lúc đóng dialog. |

Trước khi nhận PR27, browser còn thực thi: sửa 2 ly 98.000 thành 1 ly 49.000, xóa dòng; nút thêm topping vô hiệu ở 3; DB 0 đơn trước xác nhận; đăng nhập Owner/System Admin/Branch Admin và Employee B; B không đọc được đơn A; logout quay về login và trang bảo vệ yêu cầu đăng nhập lại. Đây là kết quả trên bản trước PR27, không tự xem là retest tất cả thao tác trên UI mới. Các ảnh cần dùng trong báo cáo là ảnh đã chụp lại theo phiên bản tương ứng, không ảnh mockup.

Chưa xác nhận đầy đủ catalog QA-S4-013 (đợi TTL tự nhiên), 017 (hai browser độc lập), 026 (đổi giá/tắt món khi Web đang mở), 041 (qua ranh giới 17:00 UTC), 044 (timeout sau khả năng commit). Các ca chỉ chạy một phần giữ Not Run; xem TestResults để biết số catalog chính thức.
