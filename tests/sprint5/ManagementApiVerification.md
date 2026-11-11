# Sprint 5 — Kiểm chứng API quản trị (SCRUM-110/111/112)

## Thông tin nguồn và trạng thái

- Kho mã nguồn: `TN13007/BranchFlow`.
- Mã nguồn nền được rà soát: `main` tại commit `bf69ad0a59943ca554ef1b9740b3da954f2a8698`. Trước khi chạy, cần kiểm tra lại commit mới nhất của `main`.
- Các thành phần liên quan: `ManagementController.cs`, `MvpService.cs`, `MvpRepository.cs`, `BranchAccessService.cs` và `AuthRepository.cs`.
- Trình chạy kiểm thử: [ManagementApiSmoke.ps1](ManagementApiSmoke.ps1).
- Môi trường cần có: API HTTPS chạy trên máy cục bộ ở cổng 7107, SQL Server cục bộ với database `BranchFlow_Test`, cùng các tài khoản demo đã được tạo.
- Trạng thái tại thời điểm bàn giao: **Chưa chạy trong môi trường hiện tại**. Môi trường làm việc này không có .NET SDK và SQL Server. Tài liệu ghi lại mục tiêu kiểm thử và kết quả mong đợi, không phải xác nhận các ca HTTP đã chạy thành công.

## Những phần đã có sẵn trong mã nguồn nền

Qua rà soát mã nguồn tích hợp, các chức năng sau đã tồn tại:

1. API lấy danh sách, tạo và cập nhật chi nhánh trong `ManagementController`.
2. Chỉ SystemAdmin được phép tạo hoặc cập nhật chi nhánh.
3. Danh sách chi nhánh được giới hạn theo phạm vi truy cập thông qua `MvpService.BranchesAsync` và `BranchAccessService`.
4. `MvpService.CreateUserAsync` chỉ cho phép tạo tài khoản có vai trò `BRANCH_ADMIN` hoặc `EMPLOYEE`.
5. BranchAdmin chỉ được cập nhật hồ sơ Employee thuộc chi nhánh mình có quyền truy cập. Nhánh xử lý không phải SystemAdmin không áp dụng các trường phân quyền, trạng thái hoạt động hoặc quyền kho do người gọi tự gửi lên.
6. Có kiểm tra phân công hiệu lực để bảo đảm mỗi chi nhánh chỉ có một BranchAdmin và mỗi Employee chỉ có một phân công hiệu lực.
7. SQL có unique index lọc để bảo vệ các phân công đang mở; API ánh xạ xung đột phân công thành HTTP 409.
8. DTO trả về từ API quản trị không khai báo trường mật khẩu hoặc mã băm mật khẩu.

Đây là kết quả rà soát mã nguồn, không thay thế cho việc gọi API và đối chiếu SQL thực tế.

Các ca ghi kho trái quyền sử dụng một chi nhánh QA tạm được tạo riêng, không dùng các chi nhánh demo WBS đang được chia sẻ. Tài khoản Employee trong fixture được cấp một dòng quyền kho chỉ phục vụ kiểm thử trên chi nhánh tạm đó, nhằm bảo đảm request bị từ chối do kiểm tra phạm vi chi nhánh. Khi dọn dữ liệu, script chỉ xóa giao dịch kho, nhật ký, số dư và quyền liên quan đến chi nhánh/hồ sơ QA tạm trước khi xóa chi nhánh đó.

## Ma trận kiểm thử

| Mã | Tác nhân | Request / thao tác | Kết quả mong đợi | Trạng thái |
|---|---|---|---:|---|
| M-001 | Không có token | GET `/api/management/branches` | 401 | Chưa chạy |
| M-002 | SystemAdmin | GET danh sách chi nhánh | 200; tất cả chi nhánh chưa bị xóa | Chưa chạy |
| M-003 | Owner | GET danh sách chi nhánh | 200; đọc được dữ liệu chi nhánh | Chưa chạy |
| M-004 | Owner | POST/PUT chi nhánh | 403; SQL không thay đổi | Chưa chạy |
| M-005 | Owner | GET danh sách tài khoản quản trị | 403; chính sách vai trò không cho Owner truy cập endpoint này | Chưa chạy |
| M-006 | SystemAdmin | POST tạo chi nhánh, sau đó PUT cập nhật | 200; response khớp SQL | Chưa chạy |
| M-007 | BranchAdmin 01 | GET danh sách chi nhánh | 200; chỉ có BF-DEMO-01 | Chưa chạy |
| M-008 | BranchAdmin 01 | GET tồn kho tại BF-DEMO-02 | 403 | Chưa chạy |
| M-009 | BranchAdmin 01 | Đổi trạng thái khả dụng tại BF-DEMO-02 | 403; toàn bộ trạng thái khả dụng SQL không thay đổi | Chưa chạy |
| M-010 | BranchAdmin 01 | POST tạo chi nhánh/tài khoản/phân công | 403 | Chưa chạy |
| M-011 | Employee A | GET tồn kho tại BF-DEMO-01 | 200 | Chưa chạy |
| M-012 | Employee A | GET tồn kho tại BF-DEMO-02 | 403 | Chưa chạy |
| M-013 | Employee A | POST tạo tài khoản/phân công | 403 | Chưa chạy |
| M-014 | SystemAdmin | Phân một BranchAdmin vào hai chi nhánh tạm | Cả hai phân công được chấp nhận | Chưa chạy |
| M-015 | SystemAdmin | Phân BranchAdmin thứ hai vào chi nhánh đã có Admin hiệu lực | 409; không tạo phân công thứ hai | Chưa chạy |
| M-016 | SystemAdmin | Tạo Employee kèm phân công ban đầu | 200; tài khoản, hồ sơ và phân công được tạo | Chưa chạy |
| M-017 | SystemAdmin | Tạo phân công hiệu lực thứ hai cho Employee đó | 409; giữ nguyên phân công ban đầu | Chưa chạy |
| M-018 | BranchAdmin | Cập nhật hồ sơ Employee trong phạm vi chi nhánh | 200; các trường hồ sơ được phép thay đổi | Chưa chạy |
| M-019 | BranchAdmin | Gửi thêm trường vai trò/trạng thái/chi nhánh/quyền kho nhằm tự nâng quyền | Vai trò, trạng thái, phân công và quyền kho không thay đổi | Chưa chạy |
| M-020 | SystemAdmin / BranchAdmin | Đọc response danh sách tài khoản quản trị | Không có trường mật khẩu hoặc mã băm mật khẩu | Chưa chạy |
| M-021 | BranchAdmin 01 | PUT hồ sơ Employee B ở Demo 02 | 403; tài khoản, hồ sơ, phân công và quyền kho trong SQL không đổi | Chưa chạy |
| M-022 | BranchAdmin 01 | POST/PUT sản phẩm, bao gồm giá | 403; toàn bộ danh mục và giá sản phẩm trong SQL không đổi | Chưa chạy |
| M-023 | BranchAdmin 01 | POST giao dịch kho trên chi nhánh QA tạm ngoài phạm vi | 403; dữ liệu kho trong SQL không đổi; dữ liệu thử được dọn | Chưa chạy |
| M-024 | Employee A | POST giao dịch kho trên chi nhánh QA tạm ngoài phân công, dù có dòng quyền kho thử | 403; dữ liệu kho trong SQL không đổi; dữ liệu thử được dọn | Chưa chạy |
| M-025 | SystemAdmin | Sau khi từ chối phân công BranchAdmin thứ hai | SQL còn đúng một phân công Admin đang mở cho chi nhánh A | Chưa chạy |
| M-026 | SystemAdmin | Sau khi từ chối phân công Employee lần thứ hai | SQL còn đúng một phân công đang mở cho Employee A | Chưa chạy |
| M-027 | BranchAdmin | Sau request thử nâng quyền hồ sơ Employee | SQL giữ nguyên vai trò, trạng thái, chi nhánh và quyền kho | Chưa chạy |
| M-028 | Owner / BranchAdmin / Employee | Sau request tạo chi nhánh/tài khoản bị từ chối | SQL không có bản ghi fixture bị từ chối | Chưa chạy |
| M-029 | SystemAdmin | Tắt tài khoản BranchAdmin đang có phân công hiệu lực | 400; tài khoản và các phân công không đổi | Chưa chạy |
| M-030 | SystemAdmin | Tạo chi nhánh trùng mã | 409; chỉ còn một bản ghi có mã đó | Chưa chạy |
| M-031 | SystemAdmin / BranchAdmin | Khóa rồi mở lại chi nhánh tạm đang được phân công | Chi nhánh khóa biến mất khỏi phạm vi; mở lại thì xuất hiện | Chưa chạy |

Các mã HTTP mong đợi được xác định theo controller và service hiện tại. HTTP 403 có thể xuất phát từ chính sách vai trò ở controller hoặc từ kiểm tra phạm vi chi nhánh ở service; khi ghi nhận kết quả cần phân biệt hai nguyên nhân này.

## Cách chạy bộ kiểm thử

Chỉ dùng một database kiểm thử cục bộ dành riêng cho mục đích này. Không chạy trên production, database demo đang được chia sẻ hoặc database cá nhân có dữ liệu cần giữ lại.

1. Tạo hoặc xác nhận database cục bộ `BranchFlow_Test`. Chỉ chạy baseline, dữ liệu tham chiếu và DemoSeed khi đây thực sự là database mới được tạo riêng cho kiểm thử. Xem [database/README.md](../../database/README.md).
2. Cấu hình User Secrets của API trỏ đến database Test. Lệnh sau chỉ thay đổi User Secrets trên máy cá nhân; không commit thông tin kết nối:

```powershell
dotnet user-secrets set 'ConnectionStrings:BranchFlowDb' 'Server=localhost;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True' --project src/BranchFlow.Api
```

3. Mở một terminal riêng để chạy API:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project src/BranchFlow.Api --launch-profile https
```

Profile HTTPS lắng nghe tại `https://localhost:7107`. Giữ terminal này hoạt động trong suốt quá trình kiểm thử. Với SQL Server dùng instance có tên, hãy cấu hình đúng tên server trong User Secrets và truyền cùng tên server cho script bằng tham số `-Server '.\SQLEXPRESS'`.

4. Mở terminal PowerShell 7+ khác tại thư mục gốc repository, nhập mật khẩu demo vào biến môi trường của tiến trình rồi chạy:

```powershell
$env:BRANCHFLOW_DEMO_PASSWORD = Read-Host 'Mật khẩu tài khoản demo'
./tests/sprint5/ManagementApiSmoke.ps1
Remove-Item Env:BRANCHFLOW_DEMO_PASSWORD
```

Mật khẩu demo phải lấy từ kênh riêng tư đã được nhóm thống nhất. Không ghi mật khẩu vào script, lịch sử lệnh, ảnh chụp màn hình hoặc Jira. Trước khi gửi request, script xác nhận database SQL đang kết nối chính xác là `BranchFlow_Test`.

Script tạo hai chi nhánh tạm có mã duy nhất, một BranchAdmin tạm, một BranchAdmin thứ hai và một Employee tạm. Script kiểm tra giá trị chi nhánh qua API và SQL, xung đột mã chi nhánh, khóa/mở chi nhánh, chống khóa Admin còn phân công, quyền theo vai trò và giới hạn phạm vi. Với request bị từ chối, runner chụp trạng thái SQL trước và sau để chứng minh dữ liệu không đổi; phần sản phẩm kiểm tra toàn bộ danh mục cùng các dòng giá. Trong khối `finally`, script đăng xuất các phiên thử nghiệm và chỉ xóa những bản ghi QA có tiền tố do chính lần chạy đó tạo ra. Nếu dọn fixture thất bại, runner kết thúc với mã lỗi khác 0 để không báo thành công giả.

Nếu không xác nhận được việc dọn dữ liệu, runner sẽ trả mã lỗi khác 0. Hãy dừng và chỉ kiểm tra các bản ghi `QA110A-*`, `QA110B-*` và `qa110.*` trong `BranchFlow_Test` trước khi chạy lại. Không reset database.

## Thông tin cần bàn giao sau khi chạy

### Chạy độc lập khi không dùng mật khẩu demo chung

```powershell
dotnet build src/BranchFlow.Api -c Release
$env:BRANCHFLOW_TEST_CONNECTION_STRING = 'Server=localhost\SQLEXPRESS;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True'
dotnet run --project tests/ManagementSmoke -c Release -- (Get-Location).Path
Remove-Item Env:BRANCHFLOW_TEST_CONNECTION_STRING
```

Yêu cầu PowerShell 7, sqlcmd, .NET HTTPS development certificate và baseline/reference/DemoSeed đã có trên Test. Harness không đổi mật khẩu hay reset DB. Nó mở API HTTPS local riêng bằng khóa ký ngẫu nhiên, tạo các phiên ngắn hạn cho bốn tài khoản demo đang active và chuyển token trong môi trường tiến trình con. JWT, session, role và scope vẫn được middleware thật kiểm tra; token/khóa không in ra log. Đây là kiểm thử quyền management, không phải đăng nhập bằng mật khẩu của các tài khoản dùng chung. Các tài khoản tạm do runner tạo vẫn đăng nhập bằng mật khẩu ngẫu nhiên thực.

Harness dừng đúng API mà nó mở và xóa các phiên do nó tạo. Runner dọn riêng fixture có mã GUID của lượt chạy; các lệnh SQL bật đầy đủ SET options cho filtered index, gồm QUOTED_IDENTIFIER. Lỗi cleanup được báo cùng nguyên nhân và làm lượt chạy thất bại. Không gọi trực tiếp `-UseSyntheticSessions` bằng token của môi trường khác.

Sau khi kiểm thử thật, cập nhật comment Jira với các thông tin sau:

- SHA source và tên nhánh chính xác.
- API base URL và tên database SQL đã xác nhận.
- Phiên bản PowerShell và .NET SDK.
- Kết quả từng ca theo cặp Mong đợi/Thực tế.
- Exit code của lệnh build/test và số lượng test chính xác.
- Xác nhận fixture tạm đã được dọn.
- Ca thất bại, bug liên quan, SHA sửa lỗi và kết quả kiểm thử lại.
- Kết quả review thực tế của Khang.

Không chuyển issue sang Done khi các ca HTTP/SQL bắt buộc chưa chạy, lỗi chưa được kiểm thử lại hoặc Khang chưa review minh chứng.
