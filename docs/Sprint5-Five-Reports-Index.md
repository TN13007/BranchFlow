# Tiếp nhận và gộp báo cáo cá nhân

WBS 5.6.6 • Công Thành • 10/11/2026. Bản tiếp nhận nội bộ; chưa đủ năm báo cáo để gộp thành bản chính thức. Không ký thay hoặc tạo nội dung đóng góp/actual work của người khác.

| WBS | Người phụ trách | Nguồn đã nhận | Tình trạng |
|---|---|---|---|
| 5.6.1 | Bá Thành Chí Khang | Chưa tìm thấy báo cáo cá nhân riêng trong repo và bộ tuần 5 được cung cấp | Chờ bản và xác nhận tác giả |
| 5.6.2 | Nguyễn Anh Trường Tài | Chưa tìm thấy bản cá nhân riêng | Chờ bản và xác nhận tác giả |
| 5.6.3 | Nguyễn Công Thành | [Mini-PM Thành](Sprint5-MiniPM-Thanh.md) | Đã soạn, chờ Thành/PM review |
| 5.6.4 | Nguyễn Phước Thuần | Chưa tìm thấy bản cá nhân riêng | Chờ bản và xác nhận tác giả |
| 5.6.5 | Nguyễn Đặng Hòa Thuận | [Mini PM 09/11](../tests/sprint4/MiniPMReport-2026-11-09.md) | Có báo cáo QA cũ; chưa mặc định là bản cuối WBS5.6.5 |

## Phần đã tiếp nhận để đối chiếu

Thành: tiếp nhận core, hoàn thiện FE, dữ liệu demo và tự kiểm tra UI/API/SQL; còn Owner đọc hồ sơ, ca gửi lặp, Android và public. Chi tiết và giới hạn nằm trong báo cáo riêng.

Hòa Thuận: báo cáo 09/11 ghi trên nhánh Thuanaj/candidate e83ccb8 và fixture fix a293ddf; có retest 401/timeout sau khi API commit đơn, 81/81 SQL-enabled suite và các smoke runner riêng. Chưa có public và stakeholder acceptance. Đây là nội dung báo cáo tác giả, không phải Thành đã chạy lại hoặc kết quả của candidate hiện tại.

Không cộng 81 test trong báo cáo Thuận với 76 unit/62 assertion HTTP của Thành. Hai báo cáo dùng candidate, fixture, thời điểm và đơn vị kiểm đếm khác nhau. Full suite 79/81 của lượt Thành cần giữ riêng và retest fixture clock/assignment.

## Quy tắc hoàn tất bản gộp

Mỗi người gửi bản có ngày, branch/SHA/PR, phần tiếp nhận/phần tự làm, test và tồn đọng. Actual hours chỉ ghi nếu có timesheet hoặc tác giả xác nhận; estimate không thay actual. Thành giữ nguyên nội dung và xác nhận tác giả, lập bảng tránh tính trùng đầu ra chung. Khang rà nguồn và số liệu, từng người xác nhận phần mình. Chưa đủ đầu vào thì 5.6.6 chưa Done.
