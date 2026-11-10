# Sprint 5 — Bàn giao gói phát hành API và Web (SCRUM-113)

## Mục đích và phạm vi

Tài liệu này hướng dẫn chuẩn bị riêng hai gói phát hành Linux dạng framework-dependent cho ứng dụng API và ứng dụng Web dùng Blazor. Quy trình **không triển khai ứng dụng lên máy chủ** và không tạo tài nguyên cloud.

API và Web được publish riêng vì đây là hai ứng dụng ASP.NET Core độc lập. Mỗi file ZIP phải chứa các file ứng dụng ngay ở thư mục gốc của gói. Gói Web phải có thêm thư mục `wwwroot/`.

## Build, test, publish và đóng gói

Chạy từ thư mục gốc repository bằng PowerShell 7+, có Git và .NET 10 SDK được chọn theo `global.json`.

Bộ kiểm thử tích hợp SQL sẽ bị bỏ qua nếu không có biến `BRANCHFLOW_TEST_CONNECTION_STRING`. Vì vậy, script đóng gói chủ động từ chối chạy nếu biến này không trỏ đến database `BranchFlow_Test` cục bộ.

```powershell
# Lấy giá trị từ kho bí mật hoặc cấu hình shell cục bộ đang dùng.
# Không in giá trị ra terminal, gửi qua chat hoặc commit vào Git.
$env:BRANCHFLOW_TEST_CONNECTION_STRING = '<chuỗi kết nối SQL Server cục bộ đến BranchFlow_Test>'

./tools/Publish-Release.ps1 -RuntimeIdentifier linux-x64

Remove-Item Env:BRANCHFLOW_TEST_CONNECTION_STRING
```

Script thực hiện lần lượt:

1. `dotnet restore BranchFlow.slnx`.
2. `dotnet build BranchFlow.slnx --configuration Release --no-restore`.
3. `dotnet test BranchFlow.slnx --configuration Release --no-build`.
4. Publish riêng `BranchFlow.Api` và `BranchFlow.Web` cho `linux-x64`, theo dạng framework-dependent.
5. Kiểm tra các file DLL, `.deps.json` và `.runtimeconfig.json` nằm ở thư mục gốc của mỗi gói.
6. Kiểm tra gói Web có `wwwroot/` không rỗng và đường dẫn trong ZIP sử dụng dấu gạch chéo xuôi.
7. Kiểm tra file appsettings sau publish không chứa chuỗi kết nối database hoặc JWT signing key đã được cấu hình.
8. Ghi mã SHA-256, dung lượng file, SHA source, runtime, trạng thái build/test và danh sách file trong ZIP vào `release-manifest.json`.

Các đầu ra:

- `artifacts/release/branchflow-api-linux-x64.zip`
- `artifacts/release/branchflow-web-linux-x64.zip`
- `artifacts/release/release-manifest.json`
- Log restore, build, test và publish trong `artifacts/release/`

Chỉ kết quả chạy thành công script mới được xem là minh chứng hoàn thành SCRUM-113. Việc rà source hoặc commit một mẫu manifest không được xem là đã build thành công.

## Cấu hình runtime cần cấp ngoài gói ZIP

API đọc cấu hình từ biến môi trường khi chạy. Khang hoặc người phụ trách triển khai phải cấp các giá trị này thông qua hệ thống cấu hình/bí mật của nền tảng lưu trữ:

| Cấu hình API | Mục đích |
|---|---|
| `ConnectionStrings__BranchFlowDb` | Kết nối đến SQL Server đích đã được phê duyệt |
| `Jwt__Key` | Khóa ký JWT, tối thiểu 32 byte |
| `Jwt__Issuer` | Định danh bên phát hành token |
| `Jwt__Audience` | Đối tượng nhận token |
| `ASPNETCORE_ENVIRONMENT` | Đặt thành `Production` |

Ứng dụng Web cần cấu hình:

| Cấu hình Web | Mục đích |
|---|---|
| `Api__BaseUrl` | URL HTTPS đầy đủ của API đã triển khai, có dấu `/` ở cuối |
| `ASPNETCORE_ENVIRONMENT` | Đặt thành `Production` |

File `src/BranchFlow.Web/appsettings.json` hiện dùng `https://localhost:7107/` cho môi trường local. Khi triển khai, phải ghi đè bằng URL API thực tế ở môi trường máy chủ. Không đưa URL public vào gói phát hành khi Khang chưa xác nhận endpoint thật.

Do gói được publish theo dạng framework-dependent, máy chủ Linux phải có .NET 10 ASP.NET Core Runtime. Việc kết thúc TLS, DNS, firewall, quản lý tiến trình dịch vụ, quyền truy cập database từ máy chủ và lựa chọn gói hosting thuộc trách nhiệm người phụ trách triển khai.

## Danh sách nghiệm thu gói phát hành

| Hạng mục | Minh chứng cần có | Trạng thái hiện tại |
|---|---|---|
| Chốt SHA source | SHA và nhánh chính xác trong manifest | Chờ chạy script |
| Build Release toàn solution | Build log và exit code 0 | Chưa chạy trong môi trường hiện tại |
| Bộ test có kết nối SQL | Test log, số lượng pass/fail/skip chính xác | Chưa chạy trong môi trường hiện tại |
| Publish API | DLL/deps/runtimeconfig ở thư mục gốc ZIP | Chờ chạy script |
| Publish Web | DLL/deps/runtimeconfig và wwwroot không rỗng | Chờ chạy script |
| Đường dẫn ZIP Linux | Không có đường dẫn chứa dấu gạch chéo ngược | Chờ chạy script |
| Cấu hình bí mật | Appsettings sau publish không chứa connection string hoặc JWT signing key | Chờ chạy script |
| Manifest | SHA-256 và dung lượng của cả hai ZIP | Chờ chạy script |
| Triển khai | URL và SHA đã triển khai do Khang cung cấp | Không thuộc phạm vi SCRUM-113 |

Không chuyển SCRUM-113 sang Done cho đến khi script đã chạy thật, các đầu ra được kiểm tra và Khang đã nhận hai gói ZIP, log cùng manifest.
