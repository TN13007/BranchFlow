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
  -ExpectedSha '<sourceCommit trong release-manifest.json>' `
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

## Trạng thái hiện tại

- URL API public: Chưa được cung cấp / Chưa xác minh.
- URL Web public: Chưa được cung cấp / Chưa xác minh.
- SHA đã triển khai: Chưa được cung cấp / Chưa xác minh.
- Kiểm thử public: Chưa chạy.
- Người triển khai thực tế: Khang, không phải Thuần.

Giữ SCRUM-115 mở cho đến khi nhận được thông tin triển khai từ Khang, chạy public smoke trên đúng SHA đó, liên kết và kiểm thử lại các lỗi (nếu có), đồng thời ghi rõ ranh giới giữa công việc chuẩn bị, triển khai và kiểm thử QA.
