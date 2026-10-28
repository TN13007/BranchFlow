# BranchFlow

Dự án nhóm môn Quản lý dự án CNTT. Khung ứng dụng .NET 10; chưa triển khai chức năng nghiệp vụ hoặc kết nối database.

## Cấu trúc

```text
BranchFlow.slnx
src/
  BranchFlow.Api/             Controller, cấu hình HTTP/Swagger và DI
  BranchFlow.Application/     DTOs, Services, Helpers, Constants
  BranchFlow.Infrastructure/  Models, Data, Repositories, UnitOfWork
  BranchFlow.Web/             Blazor Web App Interactive Server + Bootstrap
tests/
  BranchFlow.Tests/           xUnit và Moq; hiện chưa có test nghiệp vụ
docs/                        Hồ sơ môn học
planning/                    Kế hoạch của nhóm
```

Tham chiếu: Api → Application + Infrastructure; Application → Infrastructure; Web → Application để dùng DTO. Web gọi API bằng HTTP khi triển khai chức năng; không gọi repository/DbContext trực tiếp.

## Chạy local

Yêu cầu .NET SDK 10.0.300 hoặc bản vá cùng dải 10.0.3xx theo global.json.

```powershell
dotnet tool restore
dotnet restore BranchFlow.slnx
dotnet build BranchFlow.slnx --configuration Release --no-restore
dotnet run --project src/BranchFlow.Api --launch-profile http
```

Mở terminal thứ hai:

```powershell
dotnet run --project src/BranchFlow.Web --launch-profile http
```

- Swagger local: [http://localhost:5107/swagger](http://localhost:5107/swagger).
- Web local: [http://localhost:5132](http://localhost:5132).
- Profile HTTPS: API 7107, Web 7132; cần chứng chỉ HTTPS dev được máy tin cậy.
- Swagger mới có tài liệu rỗng vì chưa có endpoint nghiệp vụ. Chỉ bật trong Development.
- Hiện chưa cần SQL Server/JWT key để chạy hai host. Chưa có đăng nhập, phân quyền hoặc dữ liệu thật.

## Package đã chuẩn bị

| Project | Package | Phiên bản |
|---|---|---|
| Api | Swashbuckle.AspNetCore | 10.2.3 |
| Api | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.11 |
| Api, Infrastructure | Microsoft.EntityFrameworkCore.Design | 10.0.11 |
| Infrastructure | Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.SqlServer | 10.0.11 |
| Infrastructure | System.Security.Cryptography.Pkcs | 10.0.13 |
| Application | Microsoft.Extensions.Identity.Core | 10.0.11 |
| Application | System.IdentityModel.Tokens.Jwt | 8.22.0 |
| Tests | Microsoft.NET.Test.Sdk | 17.14.1 |
| Tests | xunit / xunit.runner.visualstudio | 2.9.3 / 3.1.4 |
| Tests | Moq / coverlet.collector | 4.20.72 / 6.0.4 |
| Tool local | dotnet-ef | 10.0.11 |

Blazor Interactive Server dùng ASP.NET Core shared framework, không cần cài package Blazor riêng. Bootstrap CSS 5.3.3 nằm trong wwwroot/lib và được nạp tại Components/App.razor; chưa cần JavaScript Bootstrap.

System.Security.Cryptography.Pkcs được ghim bản đã vá vì Microsoft.Data.SqlClient kéo phiên bản cũ có cảnh báo NU1902/NU1903. Xem [Microsoft security advisory](https://github.com/advisories/GHSA-846f-f764-76mx). Không tắt NuGet audit để che cảnh báo.

API có UserSecretsId riêng; chưa lưu secret nào. Khi triển khai DB/auth, cấu hình connection string và JWT key qua User Secrets hoặc môi trường host, không commit vào appsettings.json. Cài package JWT chưa đồng nghĩa API đã có xác thực.

Thư mục rỗng dùng .gitkeep để Git giữ cấu trúc; thay bằng file thực tế khi bắt đầu module. Template Class1/WeatherForecast/test rỗng đã bỏ để tránh hiểu nhầm là chức năng hoặc test hoàn chỉnh.
