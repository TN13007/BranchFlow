# Phân tích vấn đề và người dùng – BranchFlow

## 1. Bối cảnh và phân tích hiện trạng

Địa điểm tham khảo là **Sâm thảo mộc Tuấn Kiệt**, cửa hàng đồ uống bán mang đi tại **Quận 1, TP.HCM**. Theo thông tin từ thành viên BA từng làm việc hai năm, cửa hàng có **một chi nhánh**. Dự án sử dụng dữ liệu demo và **không đại diện cho hoạt động kinh doanh chính thức** của cửa hàng.

| Hạng mục theo mẫu môn học | Phân tích |
|---|---|
| **Current situation** | Nhân viên nhận món bằng miệng, người khác nhập POS; quản lý ghi sổ doanh thu cuối ca; nhân viên kiểm đếm kho và nhắn hàng thiếu cho quản lý. |
| **Problem** | Rủi ro quên món khi đông khách, tốn thời gian hỏi lại; kiểm kho mất công, có thể bỏ sót; chưa chủ động xem được chi tiết lương trước kỳ trả. |
| **Target users** | Owner, Admin/quản lý và Employee/nhân viên. |
| **Impact** | Có thể thiếu món trên hóa đơn, kéo dài thao tác phục vụ, chậm báo thiếu hàng và hạn chế tính minh bạch thông tin nhân viên. Chưa có số liệu định lượng. |
| **Current solution** | Máy POS tại quầy, sổ doanh thu, kiểm đếm thủ công, tin nhắn nhập hàng, xác nhận ca bởi quản lý và camera. |
| **Proposed solution** | Web quản lý đa chi nhánh; nhân viên tạo order qua điện thoại; quản lý menu, kho, nhân viên, bảng lương; Owner xem báo cáo. |
| **Expected value** | Hạn chế truyền miệng, hỗ trợ theo dõi kho/doanh thu, cải thiện khả năng xem thông tin lương. Kết quả cần kiểm thử, chưa phải hiệu quả đã đạt. |

## 2. Problem Statement (150–250 từ)

Sâm thảo mộc Tuấn Kiệt là cửa hàng đồ uống bán mang đi tại Quận 1, TP.HCM. Theo trải nghiệm hai năm làm việc của thành viên BA, khi phục vụ khách, nhân viên nhận yêu cầu bằng lời nói rồi chuyển cho người tại quầy nhập POS. Vào giờ đông khách, người nhận đơn có thể quên món, phải hỏi lại và dẫn đến thiếu sản phẩm trên hóa đơn. Việc kiểm kho dựa chủ yếu vào quan sát và đếm trực tiếp; nhân viên thông báo mặt hàng gần hết qua tin nhắn để quản lý quyết định nhập thêm. Kiểm kê đôi khi phải chờ ca khác hỗ trợ. Cuối ca, quản lý tổng hợp doanh thu vào sổ; nhân viên khó chủ động xem chi tiết bảng lương trước kỳ trả. Những hạn chế này ảnh hưởng đến nhân viên và quản lý, nhưng mức độ chưa được kiểm chứng độc lập. Nhóm đề xuất BranchFlow, một hệ thống web nội bộ hỗ trợ tạo đơn trên điện thoại, quản lý menu và kho, tổng hợp doanh thu, xem lương theo phân quyền và mở rộng nhiều chi nhánh bằng dữ liệu mẫu. Giá trị kỳ vọng là giảm thao tác truyền đạt thủ công và giúp người dùng theo dõi thông tin thuận tiện hơn. Hiệu quả thực tế cần được kiểm thử trước khi kết luận.

## 3. Nhóm người dùng mục tiêu

| Vai trò | Nhu cầu | Giới hạn/phân quyền | Ưu tiên |
|---|---|---|---|
| **Owner** | Xem kết quả kinh doanh, đơn hàng, kho, nhân viên, lương tổng hợp theo chi nhánh. | Chỉ xem; không tạo/sửa/xóa dữ liệu. | Mobile-first |
| **Admin** | Điều hành chi nhánh, quản lý menu, nhân viên, kho, lương, điều chỉnh đơn cần xử lý. | Trong phạm vi được phân công; nghiệp vụ quan trọng cần có lịch sử. | Desktop-first |
| **Employee** | Tạo đơn, xem đơn của mình, ghi nhận xuất/kiểm kho và xem lương cá nhân. | Không tự sửa đơn đã lưu hoặc lương/hồ sơ nhân sự. | Mobile-first |

## 4. Phạm vi suy luận và kiểm chứng

- **Đã có cơ sở từ trải nghiệm:** Các bước order qua lời nói và POS, kiểm đếm kho, báo thiếu qua tin nhắn, ghi sổ doanh thu, thông tin lương cuối tháng.
- **Giả định chưa kiểm chứng:** Quy trình hiện nay vẫn giống thời BA làm việc; tần suất lỗi; số giờ tiết kiệm; mức cải thiện doanh thu, tồn kho và sự hài lòng.
- **Chủ đích demo:** Một chi nhánh thật làm bối cảnh; các chi nhánh còn lại là **giả lập** để kiểm thử mô hình đa chi nhánh.

Chi tiết nguồn thông tin và giả định được lưu tại [`evidence-assumptions.md`](evidence-assumptions.md).

**Nguồn:** Trải nghiệm trực tiếp do BA cung cấp; yêu cầu trình bày tại `BaiTapNhom_Chuong1.pdf`, mục 2.2.
