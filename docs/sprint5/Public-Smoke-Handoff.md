# Sprint 5 — Bàn giao kiểm thử sau triển khai public (SCRUM-115)

## Phân công trách nhiệm

- Thuần chuẩn bị gói minh chứng phát hành và quy trình kiểm thử trong tài liệu này.
- Khang trực tiếp triển khai ứng dụng, đồng thời cung cấp URL API, URL Web và SHA release đã triển khai.
- Thuận / QA thực hiện kiểm thử public sau khi nhận đủ thông tin triển khai.
- Kết quả trên Test/Dev local không phải minh chứng triển khai public. Kiểm thử local thành công không đồng nghĩa ứng dụng đã có URL public hoặc đã được stakeholder nghiệm thu.

## Thông tin cần người phụ trách triển khai cung cấp

| Đầu vào | Giá trị cần có |
|---|---|
| API base URL | Địa chỉ HTTPS thực tế của API đã triển khai, ví dụ hostname được nhóm phê duyệt |
| Web base URL | Địa chỉ HTTPS thực tế của ứng dụng Web đã triển khai |
| SHA đã triển khai | SHA source chính xác do Khang cung cấp |
| SHA kỳ vọng | Giá trị `sourceCommit` trong release manifest |
| Thông tin đăng nhập demo | Mật khẩu demo hiện có, gửi qua kênh riêng tư đã được nhóm thống nhất |
| Gói phát hành | Hai file ZIP API/Web và `release-manifest.json` |

Runner từ chối URL HTTP, localhost/loopback và SHA đã triển khai khác SHA trong release manifest.

## Chạy kiểm thử HTTP public

Chạy từ repository đã checkout bằng PowerShell 7+:

```powershell
$env:BRANCHFLOW_DEMO_PASSWORD = Read-Host 'Mật khẩu tài khoản demo'

./tests/sprint5/PublicSmoke.ps1 `
  -ApiBaseUrl 'https://<hostname-api-da-trien-khai>' `
  -WebBaseUrl 'https://<hostname-web-da-trien-khai>' `
  -ReleaseManifest './artifacts/release/release-manifest.json' `
  -ExpectedSha '<sourceCommit đầy đủ 40 ký tự trong release-manifest.json>' `
  -DeployedSha '<SHA chinh xac do Khang cung cap>'

Remove-Item Env:BRANCHFLOW_DEMO_PASSWORD
```

Runner kiểm tra:

- Trang gốc Web trả về HTML qua HTTPS; PowerShell xác minh chứng chỉ và tên miền của máy chủ, không bỏ qua lỗi chứng chỉ hết hạn/không hợp lệ.
- API trả HTTP 401 khi gọi endpoint danh sách chi nhánh mà không có bearer token.
- SystemAdmin và Owner đọc được dữ liệu chi nhánh demo; Owner không thể tạo chi nhánh.
- BranchAdmin 01 chỉ truy cập được Demo 01, không đọc được tồn kho Demo 02 và không tạo được phân công.
- Employee A chỉ truy cập được Demo 01, không đọc được tồn kho Demo 02 và không tạo được tài khoản.
- Các phiên kiểm thử được đăng xuất trong khối `finally`.
- Kết quả JSON được ghi vào `artifacts/public-smoke/`; file chứa SHA được cung cấp, URL public và kết quả Mong đợi/Thực tế, không chứa token hoặc mật khẩu.

Đây là kiểm thử HTTP smoke. Runner không thực hiện browser E2E, không đối chiếu dữ liệu SQL public, không xác nhận khả năng backup/restore và không thay thế nghiệm thu của stakeholder. Những phần đó cần quy trình và minh chứng riêng.

Runner đọc `sourceCommit` từ manifest được bàn giao và so với ExpectedSha/DeployedSha trước khi gửi request. DeployedSha vẫn do Khang cung cấp từ nhật ký triển khai; đây không phải chứng thực mã nguồn từ máy chủ. Payload thử Owner tạo chi nhánh cố ý không hợp lệ để không tạo dữ liệu nếu kiểm tra quyền bị hồi quy.

## Triển khai ngày 14 tháng 11 năm 2026

- Khang triển khai Web tại https://branchflow-team-web-khang153.azurewebsites.net/login và API tại https://branchflow-team-api-khang153.azurewebsites.net.
- Candidate ứng dụng là `28441013318ffee1a6b8541f1318dab9ca124201`. Release manifest và hai ZIP được tạo bằng `tools/Publish-Release.ps1`.
- Azure SQL `BranchFlow_GroupDemo` nằm trên logical server riêng của nhóm, dùng Free với AutoPause khi hết hạn mức. Web/API dùng plan B1 hiện có.
- Bộ seed có 72 món, 14 topping và 180 đơn giả lập bổ sung. `tools/Seed-AzureGroupDemo.ps1 -Mode Verify` kiểm tổng tiền, ngày UTC+7, lịch sử và số dư kho.
- Bản trước `eb71097` đã có browser tạo đơn. Candidate mới đã qua public HTTP smoke 27 kiểm tra tại [27 kiểm tra public](../../tests/sprint5/release-2026-11-14/public-smoke.json).
- Browser của candidate mới đăng nhập Employee, chọn món và tạo đơn `BF-DEMO-01-20261114-0007`, mở chi tiết `/orders/364`.
- Owner đọc nhân viên qua `GET /api/management/employee-directory?branchId=...`. Dữ liệu gồm mã nhân viên, họ tên, chức vụ, chi nhánh và trạng thái. API quản trị tài khoản tiếp tục áp dụng quyền admin.
- UAT ba người nội bộ và xác nhận stakeholder đang chờ kết quả từ người thực sự tham gia.

Gói rollback trước candidate được lưu ở máy triển khai trong thư mục dữ liệu ứng dụng Windows `BranchFlow-TeamDemo-Deploy`. Redeploy hai ZIP API/Web của bản trước khi cần rollback ứng dụng. Backup/PITR Azure SQL được quản lý trên database riêng. Việc dọn môi trường nhóm thực hiện ở resource group `rg-branchflow-team-demo` sau báo cáo.

## Kiểm chứng local của candidate

Release build có 0 cảnh báo và 0 lỗi. Bộ SQL-enabled xUnit đạt 95/95 với 0 skipped. ManagementSmoke đạt 72 kiểm tra, OrderAdjustmentSmoke cross-day đạt 49 kiểm tra và RevenueSmoke đạt 11 kiểm tra. Các runner đã dọn fixture của từng lượt.

Ca U26 API local kiểm giới hạn trường trả về, lọc chi nhánh, chặn ID chi nhánh ngoài phạm vi và chặn Owner gọi API quản trị tài khoản. Browser public đã mở danh sách nhân viên và lọc một chi nhánh, có ảnh [ảnh U26](../../tests/sprint5/release-2026-11-14/u26-owner-directory.png).

Lượt triển khai ban đầu gặp HTTP 500 trong thời gian cập nhật file và khởi động container. Hai ứng dụng nhóm được cấu hình chạy từ ZIP package. Public smoke chỉ được ghi nhận đạt sau khi cả hai endpoint khởi động thành công.
