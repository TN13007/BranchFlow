# Phiếu tổng duyệt WBS-5.1.2

Người thực hiện: Công Thành, có công cụ hỗ trợ. Ngày chạy: 10/11/2026.
Candidate ứng dụng: bf69ad0 (main). SQL: localhost/BranchFlow_Test.
API: https://localhost:17217; Web: https://localhost:17232; Chrome Windows.
Đây là tự kiểm chứng local; chưa có chữ ký QA/người tham gia UAT hoặc smoke public.

| Thao tác thật | Expected | Actual | Kết quả |
|---|---|---|---|
| Chạy Seed-WbsDemo.ps1 hai lượt | Không tạo trùng, không đổi tài khoản hoặc fixture đã có | 4 đơn, 6 phiếu; ID đơn 20–23 giữ nguyên; fingerprint tài khoản khớp | Pass |
| Chạy verifier API với mật khẩu hiện có | Bốn vai trò, scope chi nhánh, snapshot và doanh thu khớp SQL | 23 assertion PASS; xem API-SQL-2026-11-10.txt | Pass |
| Chạy Verify-MenuOrders.sql | 5 món M/L và availability đúng 3 chi nhánh; 4 fixture đúng | Hai nhóm kiểm tra PASS; xem SQL-Manifest-2026-11-10.txt | Pass |
| EmployeeA đăng nhập, mở tạo đơn, lọc Menu demo BranchFlow | 5 món đang bán | 5 món, món tạm ngưng không xuất hiện trong danh mục bán | Pass |
| Mở Trà đào cam sả, chọn M, 3 Trân châu + 1 Thạch, tăng lên 2 ly | Preview 98.000đ | Preview 98.000đ, nút tăng Trân châu giới hạn ở 3; đóng dialog không tạo đơn | Pass |
| EmployeeA tra đơn 0006/0007/0008, mở từng chi tiết | Hoàn tất 98.000đ / Đã hủy 147.000đ / Cần xem lại 49.000đ | Cả ba mã, trạng thái, số ly và cấu hình khớp API/SQL | Pass |
| Đăng xuất A, đăng nhập EmployeeB, tra đơn CN02 mã 0001 | Một đơn mẫu của B, 1 ly M không topping, 30.000đ | ID 23, Hoàn tất, 30.000đ | Pass |
| Đăng xuất B | Kết thúc phiên kiểm chứng | Hiện thông báo đã đăng xuất | Pass |
| Build Web Release | Thành công | 0 warning, 0 error; xem Build-2026-11-10.txt | Pass |
| Thuận QA retest / Khang review | Review bởi người được giao | Chờ bàn giao | NotRun |
| Smoke public | Có URL/build triển khai thật | Chưa có URL public trong lượt này | NotRun |

Ảnh thao tác mới ở docs/evidence/WBS-5.1.2. Ảnh preview chụp phần cuối dialog; ảnh chi tiết đơn thể hiện snapshot cấu hình. Không có ảnh dựng.

Các trạng thái đơn là dữ liệu fixture trực tiếp từ SQL, không phải bằng chứng đã chạy quy trình chuyển trạng thái. Doanh thu riêng hai đơn COMPLETED là 128.000đ; không tính CANCELLED/NEEDS_REVIEW. Không seed bộ 62 món tùy chọn, không reset mật khẩu, không ghi DB Dev, không ký thay người review.

Lần dùng mật khẩu được cung cấp ban đầu trả 401; sau khi tra lại thông tin đã dùng trong lịch sử phiên, đăng nhập thật thành công. Không ghi giá trị mật khẩu vào tài liệu/log.
