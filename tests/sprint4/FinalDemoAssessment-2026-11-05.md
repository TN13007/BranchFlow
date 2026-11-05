# Kết quả bản demo giai đoạn 1 ngày 05/11/2026

Nguồn: nhánh khang sau tích hợp PR #26 (Tài) và #27 (Thành), bổ sung API đơn và sửa tích hợp của Khang với hỗ trợ AI. Không ghi nhận agent thực thi là review độc lập của Thuận.

## Kết quả đã chạy

| Tập kiểm tra | Kết quả | Bằng chứng |
|---|---|---|
| Build Release | 0 warning, 0 error | `dotnet build BranchFlow.slnx -c Release` |
| Unit tests | 65/65 Pass | `dotnet test tests/BranchFlow.Tests -c Release` |
| Auth API/SQL | 13/13 ca catalog Pass | AuthSmokeResult-2026-11-05-FinalDemo.txt |
| Menu API/SQL | 34/34 kiểm tra Pass | MenuSmokeResult-2026-11-05-FinalDemo.txt |
| Order API/SQL | 101 kiểm tra Pass | OrderSmokeResult-2026-11-05.txt |
| Seeder tài khoản/menu của thành viên | Chạy thành công trên Dev; kiểm 9 tài khoản và phạm vi | DemoSeedResult-2026-11-05.txt |
| Browser → API → SQL | Đơn 0002, 98.000đ, ID 2 khớp DB | BrowserDemoResult-2026-11-05.md; docs/evidence/sprint4-final |

Các kiểm tra order bao gồm null/giỏ rỗng, số lượng sai, topping 0/4/trùng, ghi chú trùng/cùng nhóm, các trường giá và metadata tự gửi ở root/nested, giá DB thay đổi, snapshot lịch sử, lỗi chi tiết rollback toàn bộ và counter, 12 đơn đồng thời, mã thứ tự 10000, quyền Owner/Employee ngoài chi nhánh và chi nhánh ngừng hoạt động. Runner dùng BranchFlow_Test và dọn fixture sau chạy; đơn demo UI được giữ trên BranchFlow_Dev để trình diễn.

## Đối chiếu catalog 44 ca

39 Pass, 0 Fail, 0 Blocked, 5 Not Run. Pass gồm 001–012, 014–016, 018–025, 027–040, 042–043. QA-S4-023/024 được retest thao tác sửa/xóa và giảm topping về 0 trên UI mới sau sửa SCRUM-108; 015/016 có thêm RoleScopeResult. Không cộng số assertion runner vào số ca catalog.

5 ca còn Not Run: 013 (TTL tự nhiên), 017 (hai browser độc lập), 026 (đổi giá/tắt món khi Web đang mở), 041 (ranh giới ngày Việt Nam), 044 (timeout sau khả năng commit). Một số nhánh của các ca này đã được kiểm tra nhưng chưa đủ toàn bộ bước; không ghi Pass từ suy luận.

## Đánh giá mục tiêu

Luồng ưu tiên đăng nhập → menu đúng chi nhánh → giỏ → server tính tiền → lưu đơn → chi tiết/lịch sử → logout đã có sản phẩm chạy và minh chứng. Có thể trình diễn giai đoạn 1. Chưa kết luận đã hoàn thành tất cả kiểm thử, các phân hệ giai đoạn 2 hoặc production/public deploy. Tiếp tục theo dõi các ca mở trong QA-S4-013/017/026/041/044; không cần đợi thành viên push thêm code để demo luồng đã xác minh.

## Bàn giao và review PR tiếp theo

Chạy theo docs/Sprint4-Demo-Runbook.md. Giữ seeder và UI của thành viên; PR mới được so với contract và kết quả này, sửa phần lệch rồi retest, không ghi đè toàn bộ bài của tác giả. Lịch Sprint vẫn 02–08/11; kết quả chốt nộp ngày 05/11, không tạo lịch sử trạng thái hoặc giờ làm trong quá khứ.
