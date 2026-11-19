# SCRUM-141 — đổi và khôi phục mật khẩu

Môi trường kiểm thử ngày 19/11/2026: SQL Server local BranchFlow_Test cho integration; API https://localhost:17108 và Web https://localhost:17133 dùng bản sao BranchFlow_UxReview cho kiểm tra giao diện. Không dùng DB cá nhân hoặc Azure trong các thử nghiệm này.

## Kết quả

- Build .NET: 0 warning, 0 error.
- `password-all.trx`: 133 passed, 0 failed, 0 skipped; 128 ca hiện có và 5 ca SQL mới trong PasswordRecoveryIntegrationTests.
- `browser-result.json`: 23 bước kiểm tra API/UI đạt; dùng tài khoản QA tạo riêng, không thay mật khẩu chín tài khoản demo.
- `security-result.json`: 19 bước kiểm tra bổ sung đạt, gồm phân quyền Owner/Admin Chi nhánh, CSRF, rate limit với Retry-After và bố cục mobile.
- Các ảnh PNG là ảnh chụp giao diện đang chạy. Vùng chứa mật khẩu tạm trong ảnh admin được che để không đưa credential vào minh chứng. Không có con trỏ chuột trong ảnh.

Nhóm 42 bước API/UI là các assertion của hai lượt thực thi, không cộng thành 42 test case nghiệp vụ độc lập hoặc kết quả UAT của thành viên. Human UAT ngày 21/11 chưa thực hiện tại thời điểm ghi nhận này.

## Chạy lại

1. Chạy migration `database/007-password-recovery.sql` trên BranchFlow_Test và DB clone phục vụ UI.
2. Set BRANCHFLOW_TEST_CONNECTION_STRING đến BranchFlow_Test; chạy `dotnet test tests/BranchFlow.Tests`.
3. Chạy API/Web HTTPS ở hai port trên, cấu hình API:BaseUrl và DB clone đúng. Có đủ tài khoản demo trong clone. Không dùng DB đang vận hành để chạy script tạo/reset tài khoản QA.
4. Các file browser-check.js và security-check.js được chạy qua `npx @playwright/cli -s=bf-password run-code --filename <file>`, với session Edge headless, ignoreHTTPSErrors chỉ áp dụng localhost. Kiểm tra Result.result phải là Pass; exit code của CLI riêng lẻ không thay thế kết quả assertion. Chờ hết cửa sổ một phút nếu chạy lại liên tiếp và gặp 429, không tắt rate limiter.
5. Xóa các tài khoản qa.password.browser.* tạo bởi chính lượt chạy đó trong DB clone, cùng hồ sơ/phiên/audit QA liên quan. Không xóa dữ liệu demo khác. Các fixture của lượt đã ghi nhận trong thư mục này đã được dọn sau khi kiểm tra.

Đây là minh chứng local, chưa xác nhận deploy Azure hoặc nghiệm thu người dùng. Chi tiết nghiệp vụ, giới hạn tài khoản hỗ trợ và hướng dẫn deploy ở [Password-Management.md](../../../docs/Password-Management.md).
