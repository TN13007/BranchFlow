# Kế hoạch WBS, lịch và Sprint — phần Công Thành

> **Cập nhật sau retest 10/11:** [Kết quả bổ sung](Sprint5-Retest-Completion.md) là trạng thái mới nhất: U25 gửi hai lần chỉ tạo một đơn; đã kiểm POST tạo đơn gặp 401/timeout, các form danh mục còn thiếu và cập nhật giá trên Edge. Full suite SQL-enabled đạt **81/81, không skip** sau sửa clock hai test. Tổng Test sau ba đơn mới là **215.020,50đ**; fixture riêng vẫn 128.000đ. Các số liệu và mục “chưa chạy” bên dưới là ghi nhận trước retest, được thay thế trong đúng phạm vi báo cáo mới. Owner/API, Android, public, QA độc lập và đầu vào báo cáo vẫn còn chờ.

WBS 5.3.3 • Chuẩn bị nội bộ 10/11/2026 • Nguồn: 04_THANH_FE.md trong bộ giao việc tuần 5; báo cáo NHOM6_Tuan4_BF.docx; các handoff mục 1–3 trong repo. Chưa cập nhật ProjectLibre/Jira hoặc nhận đề bài mới từ giảng viên.

## Cơ sở kế hoạch

Thành nhận 20 hoạt động, 40 giờ kế hoạch. Bảng nguồn trải từ 06–25/11, nằm trong giai đoạn 06–26/11; hạn nội bộ mới là 13/11. Các ngày nguồn được giữ để đối chiếu, không biến thành ngày thực tế hoặc cam kết mới. Tổng 103 hoạt động còn lại/191 giờ là số kế hoạch nhóm trong báo cáo trước, không phải số giờ nhóm còn lại đã đo ở thời điểm hiện tại. Không cộng 62 giờ Sprint 4 hoặc 438 giờ nguồn vào 191 giờ.

| Nhóm | WBS | Giờ KH | Tình trạng có minh chứng |
|---|---|---:|---|
| FE/API | 3.4.2.1, 3.6.2.3, 3.6.2.4, 3.5.1.5, 3.2.2.5, 3.4.3.1, 3.3.1.5 | 14 | Có sửa/kiểm tra local; Owner đọc hồ sơ còn chờ contract, một số ca UI chưa chạy |
| Dữ liệu demo | 5.1.1, 5.1.2, 5.1.3 | 6 | Đã seed/rerun Dev/Test, giữ tài khoản và bộ dữ liệu; có đối chiếu API/SQL/UI Test |
| UAT/tương thích | 4.4.1, 4.6.2, 4.6.5 | 6 | Có kịch bản và lượt local; còn gửi lặp, Android, luồng ghi Edge và public |
| Hồ sơ/hướng dẫn | 5.3.3, 5.2.3, 5.6.3, 5.3.5, 5.3.1, 5.6.6, 5.3.6 | 14 | Soạn và rà tài liệu nội bộ; gộp năm báo cáo chưa đủ đầu vào |
| Tổng | 20 hoạt động | 40 | Chưa quy đổi thành phần trăm Done |

## Lịch nguồn để đối chiếu Gantt

Mỗi dòng dự kiến 14:00–16:00, 2 giờ. Đây là bảng lịch nguồn, không phải ảnh hoặc dữ liệu xuất Gantt mới.

| Ngày KH | WBS | Đầu ra |
|---|---|---|
| 06/11 | 5.1.1 | Ba chi nhánh, bốn vai trò |
| 07/11 | 3.4.2.1 | Hiển thị điều chỉnh đơn |
| 08/11 | 4.4.1 | Kịch bản UAT |
| 09/11 | 5.1.2 | Menu/đơn mẫu |
| 10/11 | 3.6.2.3 | Lọc doanh thu theo quyền |
| 11/11 | 3.6.2.4 | Trạng thái trống/lỗi |
| 12/11 | 5.1.3 | Kho/doanh thu mẫu |
| 13/11 | 3.5.1.5 | Màn kho/phiếu |
| 14/11 | 4.6.2 | Responsive |
| 15/11 | 4.6.5 | Tương thích |
| 16/11 | 3.2.2.5 | Hồ sơ/quyền Owner |
| 17/11 | 3.4.3.1 | Lịch sử Employee |
| 18/11 | 3.3.1.5 | Quản lý menu tổng |
| 19/11 | 5.3.3 | Báo cáo WBS/Gantt/Sprint |
| 20/11 | 5.2.3 | User Guide |
| 21/11 | 5.6.3 | Mini-PM Thành |
| 22/11 | 5.3.5 | Slide và phân vai |
| 23/11 | 5.3.1 | Chín hồ sơ tuần 2 |
| 24/11 | 5.6.6 | Gộp báo cáo cá nhân |
| 25/11 | 5.3.6 | Rà soát cuối |

Các chuỗi phụ thuộc cần giữ: 5.1.1→5.1.2→5.1.3→5.5.2 (Thuần); 3.6.2.3→4.6.2→4.6.5; 5.2.2 (Khang)→5.2.3→5.2.4 (Tài); năm báo cáo 5.6.1–5.6.5→5.6.6→5.3.6. Rà soát 5.3.6 còn nhận 5.3.4 của Khang và slide 5.3.5. Gantt hiện hành cần PM xuất lại từ file ProjectLibre đã xác nhận; không tự đổi quan hệ hoặc dựng ảnh lịch mới.

## Kết quả Sprint và giới hạn so sánh

Báo cáo tuần 4 cập nhật 05/11 ghi Sprint 4 (02–08/11), 29/31 hoạt động gốc Done và 41,10% theo trọng số estimate nguồn. Hai số này là lịch sử tại mốc báo cáo, không phải tỷ lệ hoàn thành phần mềm hôm nay. Trạng thái Sprint/Jira hiện tại chưa được truy vấn lại trong mục tài liệu này.

Kết quả mới của Thành có build Release 0 lỗi/cảnh báo, 76 unit không Integration đạt, 62 kiểm tra API FE và 23 kiểm tra dữ liệu demo theo log riêng. Các loại kiểm tra dùng đơn vị khác nhau; không cộng thành tổng ca QA hoặc tỷ lệ nghiệm thu. Full suite ở mục 1 có 79/81 đạt; hai ca clock/assignment chưa được retest lại, dù báo cáo QA 09/11 ghi 81/81 trên candidate/fixture khác.

## Điều chỉnh cần PM chốt trước 13/11

Ưu tiên chốt Owner đọc hồ sơ, hoàn tất ca gửi lặp, QA độc lập và smoke public khi có deployed SHA/HTTPS. Chuẩn bị tài liệu song song với chờ đầu vào. Chưa có timesheet đáng tin để tính giờ đã làm/remaining effort; estimate 40 giờ không phải actual. Từ 11–13/11 chỉ có tối đa 6 giờ theo trần 2 giờ/ngày, chưa đủ căn cứ khẳng định mọi ca còn lại sẽ kịp. PM cần chốt người hỗ trợ/phạm vi và cập nhật lịch, không tự chuyển toàn bộ sang Done.

Minh chứng: [FE](Sprint5-FE-Section1-handoff.md), [dữ liệu](Sprint5-Demo-Data.md), [UAT](Sprint5-UAT-Responsive.md). BF-NB là mã nội bộ, không phải issue Jira mới; chưa có PR riêng cho working tree này.
