# WBS-3.4.2.1 — Bổ sung đủ phần tự kiểm chứng

Em là Công Thành, thực hiện với công cụ hỗ trợ ngày10/11/2026 trên nhánh **feat/SCRUM-124-wbs-3.4.2.1-handoff**. Bổ sung tiếp commit a556bcc; không tạo nhánh khác, không sửa ứng dụng/schema hoặc commit thay đổi task khác. Jira SCRUM-124, Story SCRUM-103. Kết quả này thay phần NotRun/xin bổ sung trace trong bàn giao trước cho đúng các ca bên dưới.

## Kết quả mới — 52 kiểm tra đạt

| Ca | Expected | Actual |
|---|---|---|
| Admin đúng branch và Admin Tổng | GET/PUT được phép | HTTP200, sửa2→3→4 ly; tổng98.000→147.000→196.000; lịch sử1→2 |
| Anonymous / Employee / Owner / Admin CN02 ghi CN01 | Không được ghi | HTTP401/403; SQL header/detail/topping/note/history/rowversion không đổi |
| Admin CN02 đọc đơn CN01 | Không lộ đơn | HTTP403 |
| Thiếu lý do, số ly0 hoặc1000 | Từ chối dữ liệu lỗi | HTTP400; SQL không đổi |
| Sửa số ly bằng RowVersion cũ | Xung đột, không ghi dở dang | HTTP409; so sánh SQL toàn bộ phần đã chụp khớp trước/sau |
| Hủy bằng RowVersion cũ | Xung đột, không hủy/ghi lịch sử | HTTP409; SQL không đổi |
| Lỗi SQL khi ghi chi tiết | Rollback header/detail/history | Inject FK sai vào OrderItem trong test interceptor, SQL547 thật; context độc lập xác nhận rollback, kể cả topping/note/rowversion |
| Lỗi SQL khi ghi lịch sử | Rollback cả phần sửa và lịch sử | Inject FK sai vào OrderAdjustment, SQL547 thật; dữ liệu trước/sau giống nhau |
| Hủy hợp lệ và xem lại | Giữ đơn, có lịch sử, Employee thấy hủy | HTTP200, CANCELLED196.000; đúng ba history; Employee GET thấy CANCELLED |
| Sửa sau hủy và hủy lần nữa | Không mở lại/không thêm history | HTTP409; SQL không đổi |
| Doanh thu sau hủy | Loại tổng của đơn đã hủy | API doanh thu thực giảm đúng196.000đ |
| Cùng mã đơn UI cũ / HTTP / SQL | Giữ minh chứng cũ và thử quyền/sửa lại | ID24/mã0009: Admin GET200; sửa sau hủy409; Employee/Owner/Admin khác branch PUT403; SQL vẫn CANCELLED147.000 và hai history |
| Snapshot sau toàn bộ lượt | Giữ size/topping/giá và note | SQL sizeM, giá món30.000, topping mỗi ly19.000, text note và structured note còn nguyên |

52 là số assertion/check của một runner, không phải52 ca UAT độc lập. Không có lần chạy fail trước lượt thành công được ghi trong báo cáo này.

## Cách chạy và giới hạn

Em bổ sung [OrderAdjustmentSmoke](../tests/OrderAdjustmentSmoke/README.md). Runner build API thật, khởi động host **Testing http://127.0.0.1:17207**, dùng SQL **localhost/BranchFlow_Test**, khóa ký ngẫu nhiên và synthetic session ngắn hạn cho tài khoản demo. Middleware vẫn kiểm chữ ký, issuer/audience, hạn token, session và role thật. Không lấy khóa của Dev, không tắt xác thực, không đổi mật khẩu. Lượt này kiểm HTTP authorization/nghiệp vụ, **không kiểm lại đăng nhập bằng mật khẩu**. Host riêng bị dừng và năm synthetic session được revoke trong finally.

Hai ca rollback gọi service/repository/UoW thật với SQL thật và test-only SaveChangesInterceptor; không phải HTTP500/browser injection. Không tạo trigger, không đổi schema hay thêm hook vào ứng dụng. EF SaveChanges tự quản lý transaction trong đúng đường production. SQL547 và việc không ghi dở dang đều được kiểm, không dùng test tạo đơn để thay cho sửa đơn.

Fixture bổ sung **WBS3421-e5356f5b170f / ID34** sao chép size/topping của đơn UI gốc, thêm ghi chú, đi hết chuỗi HTTP/SQL và được giữ CANCELLED196.000 cho QA. Mã WBS3421-* và sequence lớn là fixture trực tiếp, không nhận là mã do luồng bán hàng cấp. ID24 và fixture demo cũ được giữ nguyên; không ghi Dev. Ảnh đã gửi trước chứng minh UI trên ID24; không nhận ảnh đó là UI của ID34 hoặc mới chụp trên host17207.

## Minh chứng / bàn giao

- [Log HTTP/SQL52checks](../tests/sprint5/WBS-3.4.2.1/Completion-HTTP-SQL-2026-11-10.txt).
- [SQL chỉ đọc để QA đối chiếu](../tests/sprint5/WBS-3.4.2.1/Verify-Completion.sql), [kết quả SQL](../tests/sprint5/WBS-3.4.2.1/Completion-SQL-2026-11-10.txt).
- Build API/runner Release0warning/error; [log build](../tests/sprint5/WBS-3.4.2.1/Completion-Build-2026-11-10.txt). Unit76/76 trước đó vẫn thuộc cùng baseline ứng dụng; không gọi52checks là full suite mới.
- [Bàn giao trước và ảnh UI](WBS-3.4.2.1-Handoff.md) giữ nguồn/thời điểm. Các hàng NotRun trước được thay bằng kết quả mới trong tài liệu này.

Em đã bổ sung phần tự kiểm chứng còn thiếu của WBS3.4.2.1. Tài nhận mã đơn/script/kết quả để tiếp WBS3.4.2.2; Thuận QA retest độc lập, Khang review thật. Chưa ký thay QA/reviewer, chưa đổi Jira Done. SCRUM-124 còn WBS3.4.3.1 và hồi quy giỏ401; không nhận cả issue/Story hoàn tất. Public chưa chạy, không phải kết quả localhost ở đây.
