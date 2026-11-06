# BranchFlow

Dự án nhóm môn Quản lý dự án CNTT, xây dựng bằng .NET 10, Blazor Web App Interactive Server, EF Core Database First và SQL Server.

Phạm vi bản demo, quyền của bốn vai trò và các chức năng hoãn nằm trong [Phạm vi MVP hiện hành](docs/mvp-scope.md).

## Cấu trúc

```text
BranchFlow.slnx
src/
  BranchFlow.Api/             ASP.NET Core API, controllers, JWT, Swagger và DI
  BranchFlow.Application/     DTOs và nghiệp vụ service
  BranchFlow.Infrastructure/  EF Core DbContext, models, repositories và UnitOfWork
  BranchFlow.Web/             Blazor Web App Interactive Server + Bootstrap
tools/
  BranchFlow.DemoSeed/        Seed tài khoản demo có thể chạy lại trên DB local
tests/
  BranchFlow.Tests/           xUnit/Moq
  AuthSmoke/                  HTTP/SQL smoke cho auth (xem README riêng)
docs/                          Hợp đồng API và tài liệu bàn giao
database/                       SQL baseline, reference data và hướng dẫn DB
```

Luồng backend: Web → HTTP API → Controller → Application Service → Infrastructure Repository/UnitOfWork → SQL Server. Web không truy cập DbContext hoặc repository trực tiếp.

## Thiết lập local

Yêu cầu .NET SDK 10 theo `global.json`, SQL Server local và Git. Dùng database riêng `BranchFlow_Dev`; QA/integration test dùng `BranchFlow_Test`. Không reset database đã có dữ liệu. Làm theo [database/README.md](database/README.md) để tạo database mới và chạy `001-baseline.sql`, `002-reference-data.sql`.

Cấu hình connection string qua User Secrets, không commit vào source:

```powershell
dotnet user-secrets set 'ConnectionStrings:BranchFlowDb' 'Server=localhost;Database=BranchFlow_Dev;Trusted_Connection=True;TrustServerCertificate=True' --project src/BranchFlow.Api
```

API auth cần thêm `Jwt:Key` (ít nhất 32 byte), `Jwt:Issuer` và `Jwt:Audience`. Hướng dẫn sinh key local, seed tài khoản, chạy API, endpoint và kiểm thử nằm tại [docs/Sprint4-Auth-Local-Setup.md](docs/Sprint4-Auth-Local-Setup.md). Không thay JWT key mỗi lần chạy: đổi key sẽ làm access token hiện có mất hiệu lực. Không đưa key, mật khẩu demo, access token hoặc refresh token vào Git/log/ảnh.

## Seed tài khoản demo

Sau khi có baseline và reference data, chạy công cụ [BranchFlow.DemoSeed](tools/BranchFlow.DemoSeed/README.md). Công cụ chỉ chấp nhận môi trường Development/Testing và SQL Server local với database `BranchFlow_Dev` hoặc `BranchFlow_Test`. Seeder không reset database; lần chạy thông thường không ghi đè mật khẩu tài khoản đã tồn tại.

## Chạy API và Web

Tại root repo:

```powershell
dotnet restore BranchFlow.slnx
dotnet build BranchFlow.slnx --configuration Release
dotnet run --project src/BranchFlow.Api --launch-profile https
```

Mở terminal thứ hai để chạy Web:

```powershell
dotnet run --project src/BranchFlow.Web --launch-profile https
```

- Swagger API: https://localhost:7107/swagger
- Blazor Web: https://localhost:7132
- Chạy `dotnet dev-certs https --trust` để tin cậy chứng chỉ local trước khi đăng nhập.

Swagger hỗ trợ gọi các endpoint công khai như login/refresh. Các endpoint yêu cầu JWT cần gửi header `Authorization: Bearer <accessToken>`; không gửi token vào query string hoặc body. Tài liệu cấu hình Bearer trên Swagger UI có thể được bổ sung riêng; việc không có nút Authorize không có nghĩa endpoint bỏ qua xác thực.

## API đã có trong lát Sprint 4

| Method | Endpoint | Mục đích |
|---|---|---|
| POST | `/api/auth/login` | Xác thực username/password và phát hành token pair |
| POST | `/api/auth/refresh` | Rotate refresh token một lần |
| POST | `/api/auth/logout` | Thu hồi phiên hiện tại |
| GET | `/api/auth/me` | Trả thông tin user hiện tại |
| GET | `/api/branches/accessible` | Danh sách branch theo quyền hiện tại |
| GET | `/api/order-menu?branchId=...` | Đọc menu của branch được phép, chỉ role EMPLOYEE |

API Employee tạo đơn, xem lịch sử và chi tiết tại `/api/orders`. API quản trị chi nhánh, người dùng, menu, kho, đơn và doanh thu tại `/api/management`. Kiểm thử theo bản tích hợp tương ứng.

## Build và test

```powershell
dotnet build BranchFlow.slnx --configuration Release
dotnet test BranchFlow.slnx --configuration Release
```

Test unit không thay thế smoke HTTP/SQL hoặc browser E2E. Ghi chính xác branch/commit, môi trường, case đã chạy và case Not Run trong handoff/Jira; không đánh dấu Pass từ source inspection hoặc mock test.

## Công nghệ và quy ước

- .NET 10, C#, EF Core SQL Server, Database First.
- Blazor Interactive Server và Bootstrap 5.3.3.
- JWT HS256; cấu hình secret qua User Secrets hoặc biến môi trường.
- DB model/DbContext là scaffold; không sửa tay hoặc scaffold ghi đè tùy tiện.
- Mỗi thành viên dùng database local riêng; không đưa connection string, mật khẩu, token hay signing key vào source.
- Không tắt NuGet audit để che cảnh báo package.
