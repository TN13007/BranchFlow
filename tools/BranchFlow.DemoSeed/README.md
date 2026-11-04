# BranchFlow.DemoSeed

Công cụ tạo dữ liệu demo có thể chạy lại an toàn cho SCRUM-70.

## Điều kiện trước khi chạy

1. SQL Server local đã có database `BranchFlow_Dev` hoặc `BranchFlow_Test`.
2. Đã chạy `database/001-baseline.sql` và `database/002-reference-data.sql`. Seeder không tự tạo schema hoặc role.
3. Dùng môi trường `Development` hoặc `Testing`.
4. Connection string được truyền qua biến môi trường, không lưu vào source code.

Ví dụ PowerShell cho SQL Server local mặc định:

```powershell
$env:DOTNET_ENVIRONMENT = "Development"
$env:ConnectionStrings__BranchFlowDb = "Server=localhost;Database=BranchFlow_Dev;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run --project tools/BranchFlow.DemoSeed
Remove-Item Env:ConnectionStrings__BranchFlowDb
Remove-Item Env:DOTNET_ENVIRONMENT
```

Nếu dùng SQL Authentication hoặc instance khác, thay connection string trong biến môi trường. Chương trình chỉ chấp nhận máy chủ local và hai database được liệt kê ở trên.

Khi chạy, nhập một mật khẩu demo dài ít nhất 12 ký tự. Ký tự được nhập sẽ không hiển thị. Có thể truyền qua biến môi trường `BRANCHFLOW_DEMO_PASSWORD` trong môi trường chạy tự động; không đưa giá trị này vào command line hoặc Git.

## Tài khoản và phạm vi

| Username | Role | Trạng thái | Chi nhánh |
|---|---|---|---|
| `demo.owner` | OWNER | Active | Tất cả 3 chi nhánh |
| `demo.systemadmin` | SYSTEM_ADMIN | Active | Tất cả 3 chi nhánh |
| `demo.branchadmin.01` | BRANCH_ADMIN | Active | BF-DEMO-01 |
| `demo.branchadmin.02` | BRANCH_ADMIN | Active | BF-DEMO-02 |
| `demo.branchadmin.03` | BRANCH_ADMIN | Active | BF-DEMO-03 |
| `demo.employee.a` | EMPLOYEE | Active | BF-DEMO-01 |
| `demo.employee.b` | EMPLOYEE | Active | BF-DEMO-02 |
| `demo.employee.locked` | EMPLOYEE | Locked | Không truy cập được |
| `demo.employee.unassigned` | EMPLOYEE | Active | Chưa được phân công |

Tất cả tài khoản **được tạo mới trong cùng một lần chạy** sẽ dùng mật khẩu vừa nhập. Mật khẩu đã lưu của tài khoản tồn tại không bị thay đổi khi chạy lại. Cần chia sẻ mật khẩu demo cho nhóm qua kênh nội bộ riêng, không qua Git hoặc ảnh chụp màn hình.

## Hành vi an toàn và tính lặp lại

- Chi nhánh được nhận diện bằng `Branch.Code`, role bằng `Role.Code`, tài khoản bằng `Username`; không dùng ID số cố định.
- Chỉ tạo bản ghi còn thiếu. Không reset database, không xóa dữ liệu và không ghi đè mật khẩu.
- Nếu bản ghi cùng mã/username có trạng thái, role hoặc phân công không khớp cấu hình demo, chương trình dừng và báo xung đột để người phụ trách xử lý thủ công.
- Tài khoản `demo.employee.unassigned` có hồ sơ nhân viên nhưng cố ý không có phân công.
- Tài khoản locked vẫn có hồ sơ và phân công tại BF-DEMO-03 để kiểm tra trạng thái khóa; truy vấn tài khoản active không trả tài khoản này.
- Mọi phân công mới dùng thời gian UTC và điều kiện hiệu lực `[EffectiveFrom, EffectiveTo)`.
- Sau khi seed, công cụ truy vấn lại tài khoản và danh sách chi nhánh của cả 9 user, so sánh expected/actual rồi mới in kết quả thành công.
