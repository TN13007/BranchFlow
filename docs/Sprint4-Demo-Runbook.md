# Bản demo giai đoạn 1 - 05/11/2026

## Phạm vi đã tích hợp

Giữ seeder bán hàng của Tài (PR #26), giao diện của Thành (PR #27), auth/branch scope và QA đã merge. Bổ sung OrderService, OrdersController và OrderRepository theo contract hiện có. Không thay code thành viên bằng một bản triển khai khác.

Luồng: `Orders/Create.razor -> OrderApiClient -> POST /api/orders -> OrderService -> OrderRepository/UnitOfWork -> BranchFlowDbContext -> SQL Server`. Giá lấy lại từ menu DB; counter và các snapshot được lưu trong cùng transaction. `GET /api/orders/mine` và `GET /api/orders/{id}` chỉ trả đơn của nhân viên trong phạm vi còn được phép truy cập.

## Chạy local trên Windows

1. Cài .NET SDK theo `global.json`, SQL Server Express và nạp schema/reference theo `database/README.md`.
2. Cấu hình `ConnectionStrings:BranchFlowDb` và JWT bằng User Secrets theo `Sprint4-Auth-Local-Setup.md`; không đưa secrets vào Git.
3. Chạy công cụ `tools/BranchFlow.DemoSeed` theo README của công cụ. Seeder của thành viên tạo 9 tài khoản, 3 chi nhánh và menu bán hàng. Mật khẩu nhập riêng, tối thiểu 12 ký tự theo quy tắc seeder; chạy lại không đổi mật khẩu đã tồn tại.
4. Có thể nạp thêm `database/003-demo-menu.sql` vào DB local Dev để có món minh họa Trà đào cam sả giá 30.000đ, topping 5.000đ/4.000đ và món tạm ngưng. Script bổ sung không thay giá/menu của seeder thành viên.
5. Tin cậy chứng chỉ local bằng `dotnet dev-certs https --trust`.
6. Mở hai terminal tại root repo:

```powershell
dotnet watch --project src/BranchFlow.Api --launch-profile https
dotnet watch --project src/BranchFlow.Web --launch-profile https
```

Web: https://localhost:7132/login. API: https://localhost:7107/swagger. Dừng process cũ trước khi build nếu DLL bị khóa. Bộ kiểm thử browser trong đợt này dùng cổng riêng 17132/17107 để không đụng phiên đang chạy của PM; cấu hình `Api__BaseUrl` của Web phải trỏ cùng API.

## Kịch bản trình diễn 5 phút

1. Employee `demo.employee.a` đăng nhập; thấy Chi nhánh Demo 01 và menu SQL thật.
2. Chọn Trà đào cam sả/M, 2 ly, Trân châu 3 phần/ly, Thạch 1 phần/ly, Ít đường/Ít đá. Tổng preview: `2 × (30.000 + 3×5.000 + 4.000) = 98.000đ`.
3. Sửa số ly thành 1 để thấy 49.000đ rồi sửa về 2; topping tối đa 3, giảm về 0 bỏ chọn. Có thể xóa và thêm lại dòng.
4. Xác nhận tạo đơn; màn chi tiết hiển thị mã chính thức, 98.000đ và Hoàn tất. Đối chiếu cùng mã với SQL và danh sách Đơn của tôi.
5. Đăng xuất, mở lại trang bảo vệ phải về login. Employee B không xem được chi tiết đơn A. Owner/Admin đăng nhập được nhưng màn quản trị hiện là phạm vi giai đoạn sau; các đường `/preview/*` ghi dữ liệu mẫu, không phải CRUD đã triển khai.

Minh chứng sau tích hợp PR #26/#27: `docs/evidence/sprint4-final/`. Đơn `BF-DEMO-01-20261105-0002`, ID 2, 98.000đ được tạo trên browser và đối chiếu trong `order-sql.txt`. Đơn 0001 là lần kiểm tra trước khi tích hợp giao diện mới, không dùng để đại diện UI mới.

## Kiểm thử và giới hạn

`dotnet test tests/BranchFlow.Tests -c Release`: 65 unit test. Runner `tests/OrderSmoke` chạy thật trên BranchFlow_Test, kiểm giá DB, thuộc tính JSON thừa, quyền, snapshot, lỗi ghi chi tiết/rollback, 12 đơn đồng thời và mã 10000. AuthSmoke/MenuSmoke dùng fixture riêng và tự dọn; không reset DB Dev hoặc cloud.

```powershell
dotnet run --project tests/OrderSmoke -c Release -- "D:\BranchFlow"
dotnet run --project tests/MenuSmoke -c Release -- "D:\BranchFlow"
dotnet run --project tests/AuthSmoke -c Release -- "D:\BranchFlow"
```

Không cộng số unit/assert HTTP vào số ca catalog. Các ca chưa thực thi đủ bước vẫn Not Run. Không cam kết chịu tải production; API chưa có idempotency key cho việc người dùng tự gửi lại POST. Web khóa gửi và không tự retry POST khi kết quả chưa rõ, hướng dẫn kiểm tra lịch sử trước khi gửi lại.

## Tiếp nhận PR thành viên sau bản demo

So sánh hành vi với DTO và expected-value/test của bản demo. Giữ service/repository/component hợp lý của tác giả; sửa phần sai quyền, tính tiền, giao dịch hoặc tích hợp. Rerun build, unit, Auth/Menu/Order smoke và một luồng browser trước khi thay bản demo. Không cherry-pick toàn bộ bản mẫu để ghi đè bài của thành viên.
