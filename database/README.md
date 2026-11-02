# Database First — BranchFlow nhóm

Baseline v1 ngày 02/11/2026: SQL Server 2019+, 35 bảng, 271 trường, 59 FK. Dùng schema nhóm đã đối chiếu source cá nhân; không chạy SQL cá nhân trực tiếp. Không có cascade delete hoặc tài khoản/mật khẩu seed.

## Tạo DB trên máy thành viên

Cần SQL Server engine, SSMS và sqlcmd; SSMS riêng lẻ không tạo được instance. Mỗi thành viên dùng BranchFlow_Dev riêng; QA dùng thêm BranchFlow_Test. Chạy trong SSMS tại instance local, chỉ tạo DB chưa tồn tại:

```sql
USE master;
IF DB_ID(N'BranchFlow_Dev') IS NOT NULL
    THROW 51000,N'DB đã tồn tại; không reset hoặc chạy đè.',1;
CREATE DATABASE [BranchFlow_Dev] COLLATE Vietnamese_100_CI_AI;
```

PowerShell tại root repo (thay `localhost` bằng `.\SQLEXPRESS` nếu đúng instance máy):

```powershell
sqlcmd -S localhost -E -C -b -d BranchFlow_Dev -i database/001-baseline.sql
if ($LASTEXITCODE -ne 0) { throw 'Baseline failed; stop.' }
sqlcmd -S localhost -E -C -b -d BranchFlow_Dev -i database/002-reference-data.sql
if ($LASTEXITCODE -ne 0) { throw 'Reference seed failed; stop.' }
dotnet user-secrets set 'ConnectionStrings:BranchFlowDb' 'Server=localhost;Database=BranchFlow_Dev;Trusted_Connection=True;TrustServerCertificate=True' --project src/BranchFlow.Api
```

Nếu sqlcmd chưa có trong PATH, dùng đường dẫn cài đặt, ví dụ `C:/Program Files/Microsoft SQL Server/Client SDK/ODBC/180/Tools/Binn/SQLCMD.EXE`. `-C`/TrustServerCertificate chỉ cho ví dụ local, không áp dụng mặc định khi deploy cloud. Không chạy `dotnet user-secrets list` để chụp minh chứng.

Baseline tự kiểm tra tên DB và từ chối DB có bất kỳ bảng người dùng nào. Toàn bộ tạo bảng/FK/index/procedure trong transaction. Không xóa DB/bảng khi lỗi. Script 002 có thể chạy lại và chỉ thêm Role/Size/IngredientUnit chưa có; không tạo user, giá menu, tồn kho hoặc đơn hàng. Dev hiện chưa có tài khoản để đăng nhập; BE auth tạo seed demo bằng PasswordHasher trong bước sau.

## Chạy kiểm tra schema

Tạo BranchFlow_Test mới bằng SSMS với cùng collation, rồi chạy 001 và 002 vào DB đó. Sau đó:

```powershell
./database/Verify-Database.ps1 -Server localhost
```

Script chỉ chấp nhận instance local và cố định DB BranchFlow_Test. Kiểm tra constraint/metadata, stale RowVersion, computed columns, transaction rollback và 12 kết nối cấp mã đồng thời. Fixture constraint rollback toàn bộ; fixture concurrency được xóa theo đúng Id/code do lần chạy tạo. Nếu fixture tồn tại từ lần trước, script dừng để người phụ trách kiểm tra, không tự xóa dữ liệu cũ. Đây là kiểm tra DB thật, chưa thay thế unit/integration test nghiệp vụ API do QA viết.

## Scaffold chung

Model/DbContext đã được sinh và nằm trong Git; các thành viên không cần tự scaffold lại để chạy. Thuần chịu trách nhiệm tổng hợp thay đổi schema, review SQL, chạy thử rồi scaffold và bàn giao; Tài không tạo một bản model khác.

Lệnh dùng khi cần sinh lần đầu, từ root repo và DB local đúng phiên bản:

```powershell
dotnet tool restore --tool-manifest ./dotnet-tools.json
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef dbcontext scaffold 'Name=ConnectionStrings:BranchFlowDb' Microsoft.EntityFrameworkCore.SqlServer --project src/BranchFlow.Infrastructure --startup-project src/BranchFlow.Api --context BranchFlowDbContext --context-dir Data --output-dir Models --no-onconfiguring
dotnet build BranchFlow.slnx -c Release
```

Không dùng `--force` tùy tiện. Lệnh trên sẽ dừng khi file đã tồn tại; khi thật sự cập nhật scaffold, Thuần phải kiểm tra Git sạch/phạm vi ghi đè, review diff rồi mới dùng tùy chọn ghi đè. Không sửa model sinh tay để chữa lỗi schema. `--no-onconfiguring` không đưa connection string vào source. API đăng ký DbContext scoped, đọc User Secrets; Web không truy cập DB.

## Giới hạn DB và quy ước service phải tuân thủ

- SQL bảo vệ PK/FK/unique/check, tiền không âm, topping 1–3, snapshot, cột tính và RowVersion.
- Filtered unique chỉ bảo vệ tối đa một phân công chưa kết thúc cho branch/employee; không ngăn tất cả khoảng lịch sử chồng lấn, không ép branch luôn có Admin, không kiểm tra role của tài khoản. Service cần kiểm tra trong transaction với khóa phù hợp khi phân công/điều chuyển.
- Mã trạng thái kỹ thuật dùng: transfer PENDING/APPROVED/REJECTED; kiểm kê DRAFT/SUBMITTED/APPROVED/REJECTED; đơn COMPLETED/NEEDS_REVIEW/CANCELLED. UI hiển thị tiếng Việt theo nghiệp vụ.
- StockTransaction.Type giữ IN/OUT/REVERSAL/COUNT_ADJUSTMENT từ source cá nhân. Kiểm tra dấu, cùng branch của giao dịch gốc, quyền kho, version tồn, duyệt kiểm kê và cập nhật nguyên tử do service đảm nhận. FK không tự xác minh cùng branch giữa Stocktake và BranchIngredient.
- `usp_GetNextOrderCode` chỉ cấp số/code, không tạo đơn hoặc kiểm tra người dùng. Service phải gọi trong cùng transaction lưu đơn; rollback phải hoàn tác cả counter. Không dùng riêng procedure rồi lưu đơn trong transaction khác.
- UTC ghi/đọc ở ứng dụng; DATETIME2 không tự lưu múi giờ. BusinessDate được server đổi sang ngày Việt Nam. RowVersion là binary, không phải thời gian.
- JWT + refresh đã chốt, hai bảng auth đã sẵn; chưa có API rotation/logout. UNIQUE TokenHash không thay thế kiểm tra token/phiên trong transaction.
- Không thêm Code First migrations. Schema thay đổi qua script có phiên bản mới; không sửa lại baseline v1 sau khi nhóm đã áp dụng.

Nguồn kỹ thuật: [EF Core reverse engineering](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/), [ASP.NET Core secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0).
