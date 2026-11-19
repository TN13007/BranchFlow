# SCRUM-131 — Biên bản UAT cho ba người tham gia

Ngày lập: 17/11/2026. Đây là **biên bản chờ người tham gia điền kết quả**, chưa phải xác nhận nghiệm thu. Kết quả kiểm tra kỹ thuật trên trình duyệt cho bốn vai trò được [ghi riêng](QA-Public-Retest-2026-11-17.md); đơn do QA tạo để kiểm tra không được tính là đơn của người tham gia UAT.

Các bước thao tác và kết quả cần kiểm tra: [Hướng dẫn UAT cho PCG](../../docs/PCG-UAT-Guide-2026-11-17.md).

## Chuẩn bị buổi kiểm thử

- Trước buổi kiểm thử, người điều phối xác nhận với Khang URL Web đang chạy và SHA của bản triển khai. URL Web nhóm cung cấp gần nhất là `https://branchflow-team-web-khang153.azurewebsites.net/login`; mã bản triển khai Khang báo gần nhất là `2844101`, chưa được đối chiếu độc lập với chương trình đang chạy.
- Mời **ba người nội bộ khác nhau**. Mỗi người dùng một phiên trình duyệt riêng và tự tạo một đơn. Tài khoản demo dùng chung không xác định được người thao tác; mỗi người cần xác nhận kết quả bằng tài khoản Jira của mình hoặc một bản ghi có thể quy thuộc.
- Không đưa mật khẩu, token hoặc cookie vào biên bản. Ghi thời gian thực tế, vai trò, chi nhánh, mã đơn, kết quả mong đợi/thực tế và nhận xét. QA có thể quan sát và hỗ trợ, nhưng không xác nhận thay người tham gia.

| Người tham gia | Thời gian thực tế (VN) | Vai trò / chi nhánh | Mã đơn tự tạo | Mong đợi / thực tế, kết quả | Liên kết xác nhận của người tham gia |
|---|---|---|---|---|---|
| 1 — chờ xác nhận | Chưa chạy | Chưa có | Chưa có | Chưa có | Chưa có |
| 2 — chờ xác nhận | Chưa chạy | Chưa có | Chưa có | Chưa có | Chưa có |
| 3 — chờ xác nhận | Chưa chạy | Chưa có | Chưa có | Chưa có | Chưa có |

## Các bước cho từng người tham gia

1. Mở URL bản triển khai đã xác nhận, đăng nhập bằng tài khoản được cấp và kiểm tra vai trò/chi nhánh hiển thị.
2. Tạo một đơn demo giá trị thấp. Trước khi xác nhận, ghi món, size, số lượng và tổng tiền dự kiến. Chỉ bấm xác nhận một lần; ghi mã đơn, tổng tiền và trạng thái do hệ thống trả về.
3. Mở **Đơn của tôi**, tìm mã vừa nhận và kiểm tra đơn xuất hiện một lần với tổng tiền đúng. Nếu hết thời gian chờ hoặc gặp lỗi 401, kiểm tra danh sách đơn trước khi thử gửi lại.
4. Người tham gia tự ghi `Pass`/`Fail` và một câu nhận xét trong xác nhận có thể quy thuộc. Nếu `Fail`, QA ghi bước tái hiện, SHA, môi trường, mong đợi/thực tế, mức ảnh hưởng và liên kết issue lỗi Jira.

Để đánh giá MVP theo bốn vai trò, người điều phối/QA ghi thêm phạm vi Owner và Admin Chi nhánh, danh mục của Admin Tổng, luồng đặt món của Employee, kho và doanh thu theo SHA đã xác nhận. [Kiểm tra kỹ thuật local](QA-SCRUM-130-2026-11-17.md) và [kiểm tra lỗi](QA-SCRUM-132-Fault-2026-11-17.md) hỗ trợ đối chiếu nhưng không thay xác nhận của PCG. Hiện tượng ở [SCRUM-140](https://khangba153.atlassian.net/browse/SCRUM-140) cần FE phân tích.

Kết quả nghiệm thu của ba người vẫn là **Chưa chạy** cho đến khi cả ba hàng có kết quả thực tế và Khang xem lại biên bản. Không tính mẫu này là số ca Pass hoặc ghi lùi ngày.
