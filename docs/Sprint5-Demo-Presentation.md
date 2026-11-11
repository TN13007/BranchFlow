# Slide demo và phân vai thuyết trình

> **Cập nhật sau retest 10/11:** [Kết quả bổ sung](Sprint5-Retest-Completion.md) là trạng thái mới nhất: U25 gửi hai lần chỉ tạo một đơn; đã kiểm POST tạo đơn gặp 401/timeout, các form danh mục còn thiếu và cập nhật giá trên Edge. Full suite SQL-enabled đạt **81/81, không skip** sau sửa clock hai test. Tổng Test sau ba đơn mới là **215.020,50đ**; fixture riêng vẫn 128.000đ. Các số liệu và mục “chưa chạy” bên dưới là ghi nhận trước retest, được thay thế trong đúng phạm vi báo cáo mới. Owner/API, Android, public, QA độc lập và đầu vào báo cáo vẫn còn chờ.

WBS 5.3.5 • Công Thành • 10/11/2026. Bộ slide 8 trang được tạo trong trình xem của cuộc trò chuyện, có nội dung chỉnh sửa được và ảnh UI thật. Bản demo local, chưa có xác nhận người dùng/public. Nền 47541ee và working tree tuần 5.

Bản lưu để bàn giao: [PowerPoint 8 trang](Sprint5-BranchFlow-Demo.pptx). Đã xem bản dựng từng slide trong trình soạn; chưa mở kiểm tra trực tiếp trong Microsoft PowerPoint. Phân vai trong speaker notes là đề xuất.

## Kịch bản 8 phút — phân vai đề xuất, chờ nhóm xác nhận

| Thời lượng | Người đề xuất | Nội dung/slide | Thao tác |
|---|---|---|---|
| 0:00–1:00 | Khang | 1–2: phạm vi và bốn vai trò | Nêu quyền, phần MVP giữ lại và phần hoãn |
| 1:00–3:00 | Thành | 3: tạo đơn | Employee A, Trà đào M, 2 ly, mỗi ly 3 Trân châu + 1 Thạch; kiểm tổng 98.000đ, tạo đúng một lần |
| 3:00–4:30 | Thành | 4: điều chỉnh/hủy | Admin CN01 mở chính đơn vừa tạo, 2→3 ly thành147.000đ, nhập lý do, lưu rồi hủy qua bước xác nhận; mở lại Employee |
| 4:30–5:45 | Thuần/Tài | 5: kho | Dùng nguyên liệu thử riêng, nhập/xuất, chặn vượt tồn, đối chiếu SQL; không thay tồn BF-WBS5-v1 |
| 5:45–6:30 | Thành | 6: doanh thu | So cùng ngày/cùng DB, chỉ COMPLETED; giải thích dữ liệu cũ có thể làm tổng lớn hơn fixture |
| 6:30–7:15 | Hòa Thuận | 7: kiểm thử | Phân biệt kết quả tự kiểm tra, QA độc lập, ca còn chưa chạy |
| 7:15–8:00 | Khang | 8: việc còn lại | Chốt review, người nhận, Android/public và điều kiện nghiệm thu |

Đây là đề xuất phục vụ tổng duyệt, không phải biên bản các bạn đã nhận vai hoặc đã tham gia thuyết trình. Chưa có tổng duyệt đủ năm thành viên.

## Chuẩn bị trước buổi demo

Chọn đúng DB Test; ghi candidate SHA/working tree, cổng Web/API và ngày kinh doanh. Kiểm tra trước tài khoản/chi nhánh, nhận mật khẩu riêng. Tạo một đơn mới qua UI; không dùng đơn CANCELLED tĩnh của seed làm bằng chứng vừa hủy. Ghi mã đơn và phiếu phát sinh để đối chiếu. Các con số 98.000→147.000 là cấu hình fixture đã kiểm tra, không cam kết mọi database cùng tổng doanh thu.

Nếu mất kết nối sau xác nhận, kiểm tra lịch sử trước khi gửi lại. Nếu môi trường không sẵn sàng, dùng ảnh ghi rõ local 10/11 thay cho thao tác live; không mô tả ảnh như lượt demo mới. Nguồn: [UAT](Sprint5-UAT-Responsive.md), [dữ liệu](Sprint5-Demo-Data.md), [User Guide](Sprint5-User-Guide.md).
