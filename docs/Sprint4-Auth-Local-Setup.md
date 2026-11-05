# Sprint 4 – Handoff Backend 1: Auth và quyền chi nhánh

Tài liệu này mô tả cách cấu hình và chạy phần auth/branch API trong nhánh `feat/scrum-73-auth-logout`. Đây là hướng dẫn local/dev, không phải cấu hình production.

## 1. Điều kiện trước

- .NET SDK 10 theo `global.json`.
- SQL Server local; đã tạo `BranchFlow_Dev` hoặc `BranchFlow_Test` và chạy `database/001-baseline.sql`, `database/002-reference-data.sql`.
- Seeder tài khoản demo đã chạy trên đúng database.
- Không sử dụng production DB hoặc database chia sẻ để chạy smoke test.

Tham khảo [database/README.md](../database/README.md) và [DemoSeed README](../tools/BranchFlow.DemoSeed/README.md).

## 2. Cấu hình API bằng User Secrets

Tại root repo, cấu hình connection string theo instance SQL Server của máy. Ví dụ default instance local:

```powershell
dotnet user-secrets set 'ConnectionStrings:BranchFlowDb' 'Server=localhost;Database=BranchFlow_Dev;Trusted_Connection=True;TrustServerCertificate=True' --project src/BranchFlow.Api
```

Cấp JWT key riêng cho máy. Chỉ chạy bước tạo key khi máy chưa có key; thay key làm mọi access token đã phát hành không còn hợp lệ:

```powershell
$jwtLocalKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set 'Jwt:Key' $jwtLocalKey --project src/BranchFlow.Api
Remove-Variable jwtLocalKey
dotnet user-secrets set 'Jwt:Issuer' 'BranchFlow.Api' --project src/BranchFlow.Api
dotnet user-secrets set 'Jwt:Audience' 'BranchFlow.Web' --project src/BranchFlow.Api
```

Không chạy `dotnet user-secrets list` để chụp hoặc chia sẻ minh chứng. Không commit User Secrets, key hoặc connection string.

## 3. Seed tài khoản demo

Ví dụ cho default SQL instance và `BranchFlow_Dev`:

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
$env:ConnectionStrings__BranchFlowDb = 'Server=localhost;Database=BranchFlow_Dev;Trusted_Connection=True;TrustServerCertificate=True'
dotnet run --project tools/BranchFlow.DemoSeed
Remove-Item Env:ConnectionStrings__BranchFlowDb
Remove-Item Env:DOTNET_ENVIRONMENT
```

Seeder yêu cầu nhập và xác nhận mật khẩu tại terminal. Mật khẩu không hiển thị khi gõ. Lần chạy bình thường chỉ tạo tài khoản còn thiếu, không đổi mật khẩu của tài khoản đã tồn tại. Chỉ dùng `--reset-demo-passwords` khi thực sự cần đặt lại 9 tài khoản demo và đã hiểu phạm vi của thao tác. Chia sẻ mật khẩu qua kênh nội bộ riêng, không ghi vào Git/Jira/log công khai.

Ma trận tài khoản/role/branch được ghi trong [DemoSeed README](../tools/BranchFlow.DemoSeed/README.md). Seeder xác định ID qua username/role code/branch code; không phụ thuộc ID số cố định.

## 4. Chạy API

```powershell
dotnet run --project src/BranchFlow.Api --launch-profile http
```

API HTTP local: `http://localhost:5107`; Swagger: `http://localhost:5107/swagger/index.html`. Giữ terminal API đang chạy; dùng terminal khác cho các lệnh HTTP. Cấu hình HTTPS dùng launch profile `https` theo README.

## 5. Endpoint và xác thực

| Method | Endpoint | Xác thực | Hành vi |
|---|---|---|---|
| POST | `/api/auth/login` | Anonymous | Login; trả access token và refresh token cùng expiry UTC |
| POST | `/api/auth/refresh` | Anonymous | Refresh rotation; token cũ chỉ dùng một lần |
| POST | `/api/auth/logout` | Bearer JWT | Thu hồi session khớp `sub` và `sid`; trả 204 sau commit |
| GET | `/api/auth/me` | Bearer JWT | Lấy user hiện tại theo `sub` |
| GET | `/api/branches/accessible` | Bearer JWT | Lấy branch theo role và assignment hiệu lực |
| GET | `/api/order-menu?branchId=...` | Bearer JWT, EMPLOYEE | Đọc menu trong branch scope |

Với request được bảo vệ, gửi HTTP header:

```http
Authorization: Bearer <accessToken>
```

GET không cần query/body để nhận user identity, nhưng vẫn cần Bearer header. Controller lấy `sub`/`sid` từ JWT đã xác thực, không nhận userId/sessionId từ client. Swagger UI hiện chưa khai báo security scheme Bearer nên có thể không hiển thị nút Authorize; dùng HTTP client/PowerShell với header như trên để kiểm tra protected endpoint.

## 6. Thời hạn và vòng đời phiên

- Access JWT: 15 phút.
- Auth session: tối đa tuyệt đối 7 ngày kể từ login.
- Refresh token: ngẫu nhiên; DB chỉ lưu SHA-256 hash; rotation không kéo dài mốc hết hạn tuyệt đối của session.
- JWT middleware xác minh chữ ký HS256, issuer, audience, lifetime với `ClockSkew = 0`, claims `sub`, `sid`, `role`, rồi kiểm tra session/user/role trong DB ở mỗi request được bảo vệ.
- Refresh token reuse làm thu hồi session chứa token đó.
- Logout chỉ thu hồi session hiện tại (khớp cả userId và sessionId) và refresh tokens trong session đó. Session khác của cùng user không bị thu hồi.

Đổi JWT key làm access JWT hiện có không qua signature validation. Khi cần kiểm tra revoke, dùng session mới và bảo đảm request được thực hiện trước expiry; không nhầm access-token expiry với session revoke.

## 7. Bộ kiểm thử và bằng chứng hiện có

Unit test trên nhánh `feat/scrum-73-auth-logout`:

```powershell
dotnet test BranchFlow.slnx --configuration Release
```

Kết quả chạy thực tế: build succeeded; **55 tests passed, 0 failed, 0 skipped**.

Manual HTTP smoke local đã xác nhận:
- Login session A, `GET /api/auth/me` trả 200.
- Logout A trả 204.
- Dùng access token A gọi lại `/api/auth/me` trả 401.
- Dùng refresh token A sau logout gọi `POST /api/auth/refresh` trả 401.
- Session B của cùng user vẫn gọi `/api/auth/me` và nhận 200.

QA độc lập trên feature branch trước đó ghi nhận 13/13 case auth sơ bộ Pass (QA-S4-001–012, trừ 013, và 014) với SQL/HTTP fixture; fixture đã được dọn. Xem [AuthSmoke result](../tests/sprint4/AuthSmokeResult-2026-11-05.txt) trên nhánh QA `Thuanaj` và runner [AuthSmoke README](../tests/AuthSmoke/README.md). Kết quả QA feature branch là bằng chứng sơ bộ, chưa phải nghiệm thu trên release candidate tích hợp.

**Chưa chạy / còn giới hạn:**
- QA-S4-013: chờ access TTL và absolute session TTL hết hạn tự nhiên với TTL test ngắn.
- QA-S4-015–016: phần create-order cần API tạo đơn và fixture tương ứng.
- QA-S4-017–018: browser E2E cần FE/API tích hợp trên cùng release candidate.
- Chưa có CI status checks gắn vào PR #21.
- Chưa có runtime bug auth được xác nhận trong BugLog tại thời điểm handoff; không tạo bug giả.

## 8. Lệnh kiểm tra nhanh

Kiểm tra unit suite:

```powershell
dotnet build BranchFlow.slnx --configuration Release
dotnet test BranchFlow.slnx --configuration Release --no-build
```

Kiểm tra API protected endpoint bằng PowerShell (không paste token vào chat):

```powershell
$token = Read-Host 'Paste accessToken'
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri 'http://localhost:5107/api/auth/me' -Method Get -Headers $headers
```

Để gọi logout, dùng đúng access token của session muốn thu hồi:

```powershell
Invoke-WebRequest -Uri 'http://localhost:5107/api/auth/logout' -Method Post -Headers $headers -UseBasicParsing
```

Mong đợi: 204. Request được bảo vệ tiếp theo với access token đó phải 401. Refresh token tương ứng cũng phải nhận 401. Không chia sẻ access token/refresh token; mọi minh chứng phải che token, password, hash và JWT key.