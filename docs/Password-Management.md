# Quản lý mật khẩu — SCRUM-141

## Phạm vi

Phản hồi PCG được áp dụng theo cơ chế khôi phục có xác minh. Admin Tổng không xem được mật khẩu đã lưu. Database chỉ giữ hash do ASP.NET Core PasswordHasher tạo; không lưu mật khẩu tạm có thể đọc lại.

- Tài khoản Admin Chi nhánh/Employee mới: Admin Tổng nhập mật khẩu ban đầu dài 12–128 ký tự. Mật khẩu này là mật khẩu tạm, hết hạn sau 24 giờ và bắt buộc đổi khi đăng nhập lần đầu.
- Người đang đăng nhập: mở Tài khoản → Đổi mật khẩu. Nhập mật khẩu hiện tại, mật khẩu mới khác mật khẩu cũ và xác nhận trùng khớp. Mật khẩu mới dài 12–128 ký tự; không tự trim mật khẩu.
- Người quên mật khẩu: mở Quên mật khẩu ở màn đăng nhập, nhập username rồi gửi yêu cầu. Tài khoản có/không tồn tại nhận cùng thông báo. Thao tác này không đổi mật khẩu, không thu hồi phiên và không gửi email.
- Admin Tổng: Tài khoản nhân viên → Đặt lại mật khẩu hoặc Hỗ trợ mật khẩu. Xác minh danh tính qua thông tin liên lạc đã biết, chọn lý do, xác nhận rồi cấp mật khẩu tạm ngẫu nhiên. Mật khẩu có hiệu lực 30 phút và buộc đổi; chỉ hiển thị trong lần cấp, giao riêng cho người được xác minh rồi bấm ẩn. Nếu mất mật khẩu tạm, cấp một mật khẩu mới.
- Owner, Admin Chi nhánh và Employee không có quyền reset cho người khác. Luồng hỗ trợ này không reset tài khoản Owner/System Admin. Tài khoản thuộc các vai trò này vẫn tự đổi khi biết mật khẩu hiện tại; khôi phục khi bị khóa ngoài cần người vận hành xác minh và thực hiện riêng.

## Phiên và dữ liệu

Chạy `database/007-password-recovery.sql` trước API/DemoSeed. Các tài khoản hiện có giữ nguyên mật khẩu và không bị bắt đổi; tài khoản demo dùng theo bộ fixture đã thống nhất. Không dùng mật khẩu demo làm giá trị reset cho người dùng.

`AppUser` bổ sung MustChangePassword, TemporaryPasswordExpiresAt và CredentialVersion (tổng 14 trường). `AuthSession` giữ bản chụp CredentialVersion. Sau đổi/reset, phiên bản credential tăng; mọi request JWT kiểm tra phiên bản với DB, nên phiên được tạo muộn từ thông tin cũ cũng bị từ chối. Refresh kiểm tra phiên bản và hạn mật khẩu tạm. Toàn bộ phiên/refresh cũ được thu hồi trong cùng transaction với đổi hash, giải quyết yêu cầu đang chờ và ghi audit.

`PasswordResetRequest` lưu Id, UserId, RequestedAt, ResolvedAt, ResolvedByUserId; hai FK đến AppUser, không cascade delete. Unique filtered index giới hạn một yêu cầu chưa giải quyết cho mỗi user. Yêu cầu đồng thời dùng range lock; không lưu dữ liệu tự do từ người gửi ẩn danh.

Audit dùng Action=UPDATE theo constraint hiện có, Event=PASSWORD_CHANGED/PASSWORD_RESET trong AfterData, kèm người thực hiện, user đích, thời gian, lý do nghiệp vụ và hạn mật khẩu tạm. Không ghi mật khẩu/hash/token. Không chụp hoặc đính kèm mật khẩu tạm vào Jira, PR hay báo cáo.

## API contract

| Method | Endpoint | Quyền / kết quả |
|---|---|---|
| POST | /api/auth/forgot-password | Anonymous; `{username}` → 200 thông báo chung; chỉ tạo yêu cầu cho Employee/Admin Chi nhánh đang hoạt động |
| GET | /api/management/password-requests | System Admin; tối đa 100 yêu cầu chưa giải quyết, cũ trước |
| POST | /api/management/users/{id}/reset-password | System Admin; `{identityVerified:true,reason}` → username, temporaryPassword, expiresAt; response no-store |
| POST | /api/auth/change-password | JWT còn hiệu lực; `{currentPassword,newPassword,confirmPassword}` → 204; mọi phiên cũ hết hiệu lực |
| GET | /api/auth/me | Bổ sung `mustChangePassword`; không trả hash, mật khẩu hay credential version |

400: nội dung không hợp lệ hoặc mật khẩu hiện tại không đúng; 401: thiếu/hết hiệu lực JWT; 403: sai quyền, phiên thay đổi trong khi thao tác hoặc PASSWORD_CHANGE_REQUIRED; 404: target không hoạt động/không tồn tại; 409: credential vừa được thao tác khác cập nhật; 429: quá 5 lần/phút theo endpoint và user/IP, có Retry-After=60.

Khi bắt buộc đổi, API chỉ cho `/api/auth/me`, `/api/auth/change-password`, `/api/auth/logout`. UI cũng chặn điều hướng đến màn nghiệp vụ. Form đổi/quên mật khẩu dùng static SSR và antiforgery, tránh mất dữ liệu lúc Blazor hydrate. Cookie bảo mật, token vẫn chỉ ở server. Web session store hiện dùng một instance; thu hồi quyền truy cập luôn được API xác minh từ DB.

## Kiểm thử và triển khai

133/133 kiểm thử .NET/SQL đạt, không skipped; trong đó năm ca SQL mới kiểm tra xác nhận mật khẩu, nhiều phiên, quyền reset, hạn mật khẩu tạm, audit, đăng nhập trễ bằng credential cũ, hai yêu cầu đổi đồng thời và reset đồng thời với refresh. 42 kiểm tra API/UI thực tế đạt, chia thành 23 kiểm tra luồng chính và 19 kiểm tra bảo vệ bổ sung: tạo tài khoản, forced change, phân quyền, response chống liệt kê tài khoản, CSRF, rate limit, hỗ trợ reset, hết hiệu lực access/refresh cũ, giao diện mobile và ẩn mật khẩu sau bàn giao. Minh chứng trong [SCRUM-141](../tests/sprint6/SCRUM-141/README.md).

Đây là thay đổi local. Trước lần deploy tiếp theo cần chạy migration 007 trên DB nhóm Azure rồi deploy cả API và Web cùng phiên bản; không chạy lên DB dự án cá nhân. Chưa có khôi phục tự động bằng email và chưa thay thế kết quả UAT do thành viên thực hiện ngày 21/11.

Tham khảo: [OWASP Password Storage](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html), [OWASP Forgot Password](https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html).
