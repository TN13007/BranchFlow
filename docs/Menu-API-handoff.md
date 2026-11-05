# Bàn giao API menu sau review

GET `/api/order-menu?branchId=...` yêu cầu JWT HS256 hợp lệ, role EMPLOYEE và phân công chi nhánh hiện còn hiệu lực. Token dùng các claim `sub` (AppUser.Id dạng số), `role` (mã role) và `sid` (AuthSession.Id dạng GUID). API kiểm tra chữ ký, issuer, audience, thời hạn, tài khoản/role và phiên DB mỗi request; phiên bị thu hồi hoặc tài khoản bị khóa trả 401. Token hợp lệ nhưng sai role/chi nhánh trả 403. Kiểm tra phạm vi trước khi truy vấn menu; chi nhánh không hoạt động trong phạm vi được cấp trả 404.

## Cấu hình local

Ngoài ConnectionStrings:BranchFlowDb hiện có, API cần `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`. Không có key mặc định trong source. Dùng user-secrets hoặc biến môi trường, không commit giá trị key. Ví dụ PowerShell tại root repo để tạo key local ngẫu nhiên, không in key ra màn hình:

```powershell
$jwtLocalKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set 'Jwt:Key' $jwtLocalKey --project src/BranchFlow.Api | Out-Null
Remove-Variable jwtLocalKey
dotnet user-secrets set 'Jwt:Issuer' 'BranchFlow.Api' --project src/BranchFlow.Api
dotnet user-secrets set 'Jwt:Audience' 'BranchFlow.Web' --project src/BranchFlow.Api
```

Chỉ tạo key khi máy chưa có cấu hình; thay key làm token hiện có mất hiệu lực. Mỗi máy local có thể dùng key riêng. Thuần dùng cùng ba cấu hình này và các claim trên khi hoàn thiện login/refresh/logout; không thêm một bộ JWT middleware thứ hai. API login/refresh được triển khai theo contract Sprint 4:
- POST `/api/auth/login`, body `{ "username": "...", "password": "..." }`; username được trim, password giữ nguyên. Thiếu trường bắt buộc trả 400; thông tin sai hoặc tài khoản khóa/xóa trả cùng lỗi 401.
- POST `/api/auth/refresh`, body `{ "refreshToken": "..." }`; endpoint không yêu cầu access JWT. Token sai, hết hạn, bị thu hồi hoặc session không còn hiệu lực trả lỗi 401 chung.
- Login giới hạn 5 lần/phút/IP; vượt giới hạn trả 429 và `Retry-After`.
- Login và refresh trả `Cache-Control: no-store`. Không ghi access token, refresh token, token hash hoặc JWT key vào log.
- Access JWT có thời hạn 15 phút và claim `sub`, `sid`, `role`. Refresh token được sinh ngẫu nhiên; SQL chỉ lưu SHA-256 hash (64 ký tự hex).
- Session có thời hạn tuyệt đối 7 ngày từ lúc login. Token refresh mới giữ nguyên `AuthSession.ExpiresAt`, không kéo dài session. Refresh token cũ chỉ dùng một lần; reuse sẽ thu hồi session và làm access JWT thuộc session đó bị middleware từ chối.

Production phải cấp cấu hình môi trường riêng.

## Flow và kiểm tra

JWT middleware → MenuAccessRepository kiểm tra phiên/tài khoản → OrderMenuController kiểm role/query → OrderMenuService kiểm branch scope qua IBranchAccessService → AuthRepository truy vấn assignment/branch active → OrderMenuRepository truy vấn dữ liệu → service ánh xạ DTO → controller.

IBranchAccessService là interface quyền chi nhánh dùng chung cho menu/order. GET /api/branches/accessible dùng cùng service; không tự viết lại bộ lọc assignment ở từng controller/service. Xem docs/Branch-Access-API-handoff.md để biết DTO và cách tích hợp.

Lỗi 400/401/403/404/500 dùng ProblemDetails có traceId; không trả stack/SQL. Repository giữ bộ lọc active/deleted/availability và CancellationToken. Không đổi schema/model/DTO FE.

Chạy build/test tại root repo. Unit test service xác minh không đọc menu sai phạm vi và ánh xạ ProductSizeId/giá/topping. Smoke SQL chạy riêng trên BranchFlow_Test, tạo và dọn fixture theo ID riêng; không dùng BranchFlow_Dev hoặc DB cloud.

```powershell
dotnet build BranchFlow.slnx -c Release
dotnet test BranchFlow.slnx -c Release --no-build --no-restore
dotnet run --project tests/MenuSmoke/MenuSmoke.csproj -c Release -- (Get-Location).Path
```

Smoke mặc định dùng localhost\SQLEXPRESS và BranchFlow_Test, cần schema/reference-data trước đó. Nếu dùng default instance, đặt `BRANCHFLOW_MENU_TEST_DB` qua biến môi trường với Database=BranchFlow_Test; script từ chối server ngoài localhost hoặc database khác. Port test 19117 phải rảnh. Key JWT được sinh ngẫu nhiên trong tiến trình và chỉ cấp cho host test; không cần sửa user-secrets để chạy smoke.
