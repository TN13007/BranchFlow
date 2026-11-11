# Mục 4 — danh mục tài liệu bàn giao

> **Cập nhật sau retest 10/11:** [Trích log retest 10/11](WBS-4.4.1/evidence/records/Source-L02-Excerpt.md) là trạng thái mới nhất: U25 gửi hai lần chỉ tạo một đơn; đã kiểm POST tạo đơn gặp 401/timeout, các form danh mục còn thiếu và cập nhật giá trên Edge. Full suite SQL-enabled đạt **81/81, không skip** sau sửa clock hai test. Tổng Test sau ba đơn mới là **215.020,50đ**; fixture riêng vẫn 128.000đ. Các số liệu và mục “chưa chạy” bên dưới là ghi nhận trước retest, được thay thế trong đúng phạm vi báo cáo mới. Owner/API, Android, public, QA độc lập và đầu vào báo cáo vẫn còn chờ.

Công Thành • 10/11/2026 • Nền 47541ee, nhánh thanhnc, chưa commit/push. Đây là gói nội bộ theo phân công tuần 5; chưa tự nhận đã hoàn tất toàn bộ nghiệm thu hoặc đã nộp bài giảng viên.

| WBS | Đầu ra | Trạng thái |
|---|---|---|
| 5.3.3 | [Báo cáo WBS, lịch, Sprint](Sprint5-WBS-Sprint-Report.md) | Đã soạn; chờ PM đối chiếu lịch/Jira hiện hành |
| 5.2.3 | [Hướng dẫn Admin CN/Employee](Sprint5-User-Guide.md) | Đã soạn theo UI; chờ người khác làm theo và nhận bản 5.2.2 của Khang |
| 5.6.3 | [Mini-PM Thành](Sprint5-MiniPM-Thanh.md) | Đã soạn; chờ xác nhận/review |
| 5.3.5 | [Kịch bản và phân vai](Sprint5-Demo-Presentation.md), [PowerPoint 8 trang](Sprint5-BranchFlow-Demo.pptx) và bản chỉnh sửa trong cuộc trò chuyện | Đã chuẩn bị; phân vai đề xuất, chưa tổng duyệt nhóm |
| 5.3.1 | [Danh mục 9 hồ sơ tuần 2](Sprint5-Week2-Dossier-Index.md) | Đã đối chiếu danh mục gốc; quyền mở nguồn online chưa xác minh |
| 5.6.6 | [Tiếp nhận 5 báo cáo](Sprint5-Five-Reports-Index.md) | Chưa đủ đầu vào để gộp chính thức |
| 5.3.6 | Bảng rà soát dưới đây | Đã rà phần local; chờ 5.3.4 của Khang và các đầu vào còn thiếu |

## Rà soát 5.3.6

| Nội dung | Kết quả |
|---|---|
| Đúng mã WBS, người làm và mốc ghi nhận | Có trong từng tài liệu; không tự gán Jira ID |
| Lịch cũ / hạn mới / actual | Tách lịch 06–25/11 khỏi hạn nội bộ 13/11; không tạo timesheet hoặc % Done giả |
| Phạm vi | Theo MVP hiện hành; không mở payroll/transfer/stocktake hoặc approval nhiều bước |
| Số liệu doanh thu | Fixture128.000đ; Test128.020,50đ sau hủy; ảnh Edge226.020,50đ ở thời điểm còn đơn98.000đ |
| Kết quả test | Tách unit, assertion HTTP, ca QA, candidate và thời điểm; giữ tồn đọng full suite79/81 |
| Minh chứng | Dùng ảnh thật có sẵn; không dùng ảnh chụp sớm/resize chưa ổn định làm chứng cứ đạt |
| Nguồn tuần2 | Giữ mô tả lịch sử bảy phân hệ; không thay bằng phạm vi hiện hành rồi gọi là bản gốc |
| Link local | Log gốc ghi lượt kiểm tra trên máy tác giả. Bản tích hợp đã kiểm tra lại các liên kết tương đối trong repo. |
| Link online/quyền nhóm | Chưa kiểm tra đăng nhập/quyền bằng tài khoản nhóm; cần chủ nguồn xác nhận |
| Nộp/review | Chưa upload, gửi tin nhắn, ký, cập nhật Jira hoặc commit/push |

## Nguồn và việc bàn giao tiếp

Phân công gốc: `C:/Users/PC/Documents/qlda tuan 5/04_THANH_FE.md`. Hồ sơ đầu vào: báo cáo tuần4 và danh mục tuần2 trên Desktop; [bàn giao FE responsive](WBS-4.6.2-Handoff.md), [dữ liệu demo kho và doanh thu](WBS-5.1.3-Handoff.md), [trích kết quả UAT 10/11](WBS-4.4.1/evidence/records/Source-L01-Excerpt.md). Bộ tài liệu này không sửa các bản Word gốc hoặc ảnh PM đã chèn.

Khang cần chốt bản kế hoạch hiện hành, mapping issue và phần tổng hợp QA/rủi ro 5.3.4. Hòa Thuận retest độc lập; mỗi tác giả gửi/xác nhận báo cáo cá nhân. Tài tiếp nhận User Guide sau review để gộp phần 5.2.4. Chỉ chốt Done/nộp bản chính thức khi đủ các đầu vào và người chịu trách nhiệm xác nhận.
