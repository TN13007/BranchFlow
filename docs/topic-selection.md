# Lựa chọn đề tài dự án – Tuần 1

**Dự án:** BranchFlow | **Nhóm:** 6  
**Trạng thái:** Nhóm đã chọn nội bộ; **chưa được giảng viên phê duyệt**.

## 1. Hai đề tài đề xuất

- **Đề tài A (được chọn):** Xây dựng hệ thống quản lý chuỗi cửa hàng đồ uống – BranchFlow.
- **Đề tài B (đối chiếu):** Website bán hàng cho hộ kinh doanh (đề tài gợi ý số 1 của giảng viên).

BranchFlow tập trung quản lý vận hành nội bộ; đề tài B tập trung cung cấp kênh bán hàng và quản lý đơn trực tuyến. Hai đề tài không đồng nhất về bài toán và đối tượng sử dụng.

## 2. So sánh theo 6 tiêu chí của bài tập

| Tiêu chí | Đề tài A – BranchFlow | Đề tài B – Website bán hàng hộ kinh doanh |
|---|---|---|
| Vấn đề có thực tế không? | Có cơ sở từ trải nghiệm vận hành tại Sâm thảo mộc Tuấn Kiệt. | Bài toán có tính ứng dụng, nhưng nhóm chưa có trường hợp hộ kinh doanh cụ thể để quan sát. |
| Người dùng mục tiêu có rõ không? | Owner, Admin và Employee, có nhu cầu nghiệp vụ khác nhau. | Chủ hộ kinh doanh, nhân viên quản lý và khách hàng. |
| Có thể khảo sát/thu thập bằng chứng không? | Có tường thuật trực tiếp của thành viên BA làm việc hai năm; chưa có khảo sát độc lập. | Cần tiếp cận hộ kinh doanh để tìm và xác minh quy trình thực tế. |
| Khả thi trong thời gian học không? | Khả thi **có điều kiện**: 7 phân hệ giới hạn ở mức demo, kiểm soát tiến độ 50 ngày. | Tương đối thuận lợi nếu chọn MVP danh mục – giỏ hàng – đơn hàng cơ bản. |
| Có thể xây dựng MVP không? | Có; phân quyền, chi nhánh, menu, order, kho, doanh thu và lương cơ bản. | Có; danh mục, tìm kiếm, giỏ hàng, đặt hàng và quản lý đơn. |
| Nhóm có đủ kiến thức thực hiện không? | Có định hướng ASP.NET Core, Blazor, Bootstrap, SQL Server; cần phân bổ công việc và kiểm chứng năng lực qua Sprint. | Có thể dùng cùng nền tảng nhưng cần làm rõ nghiệp vụ bán hàng trực tuyến. |

**Kết luận:** Nhóm chọn **BranchFlow**, ưu tiên vấn đề đã có bối cảnh gần thực tế và người dùng mục tiêu xác định rõ. Đề tài **chờ giảng viên chấp thuận** vì nằm ngoài danh sách gợi ý.

## 3. Trả lời 5 câu hỏi bắt buộc

### 3.1. Vì sao nhóm chọn BranchFlow thay vì đề tài khác?

Một thành viên phụ trách BA từng trực tiếp làm việc hai năm tại Sâm thảo mộc Tuấn Kiệt, nên có thể mô tả cụ thể quy trình bán hàng, kiểm kho, doanh thu và thông tin bảng lương. Nhóm chọn đề tài nhằm giải quyết các bất cập đã quan sát, đồng thời thực hành phân tích, thiết kế và quản lý một dự án phần mềm có nhiều vai trò sử dụng. Nhóm nhận thức được rủi ro phạm vi lớn so với 50 ngày và chỉ cam kết MVP ở mức demo.

### 3.2. Vấn đề thật hay chỉ là giả định?

Các tình huống nhận order bằng lời nói, nhập POS, kiểm kho thủ công và ghi sổ doanh thu có nguồn từ kinh nghiệm trực tiếp **trong quá khứ** của BA. Tuy nhiên, tần suất sai sót, quy trình vận hành **hiện nay** và mức hiệu quả kỳ vọng sau số hóa **chưa được kiểm chứng độc lập**. Không coi các nhận định này là kết quả khảo sát toàn nhóm.

### 3.3. Ai đang gặp vấn đề?

Nhân viên nhận/nhập đơn có thể bỏ sót món khi đông khách; nhân viên kiểm kho phải đếm và báo thiếu thủ công; quản lý tổng hợp sổ cuối ca và quyết định nhập hàng; nhân viên khó theo dõi bảng lương trước kỳ trả; chủ cửa hàng cần thông tin kinh doanh tập trung. Trong bản demo, các vai trò được mô phỏng bằng Owner, Admin và Employee.

### 3.4. Nhóm có thể thu thập bằng chứng nào?

Hiện có **tường thuật có cấu trúc của thành viên BA** và sơ đồ/mô tả quy trình được lập từ kinh nghiệm làm việc. Nhóm **không thực hiện phỏng vấn bổ sung** và chưa có dữ liệu nội bộ được cho phép công bố. Nếu cần kiểm chứng tiếp, nhóm phải xin phép phù hợp trước khi phỏng vấn người đang làm việc hoặc sử dụng tài liệu nội bộ.

### 3.5. Nếu không có BranchFlow, cửa hàng đang xử lý thế nào?

Nhân viên tiếp nhận order bằng lời nói, đọc cho người tại quầy nhập POS. Cuối ca, quản lý tổng hợp doanh thu và ghi sổ. Kiểm kho thực hiện bằng quan sát/đếm trực tiếp, rồi báo mặt hàng gần hết qua tin nhắn để quản lý quyết định nhập hàng. Ca làm được quản lý xác nhận, có đối chiếu camera. Đây là mô tả ở thời điểm BA từng làm việc, không mặc nhiên xác nhận là trạng thái hiện tại.

## 4. Trạng thái cần theo dõi

- [x] Nhóm đã lựa chọn BranchFlow theo xác nhận của PM.
- [ ] Được giảng viên phê duyệt đề tài ngoài danh sách gợi ý.
- [ ] Có nguồn kiểm chứng độc lập về quy trình hiện tại (chưa bắt buộc phải có tại thời điểm soạn; nếu không có thì nêu giới hạn).

**Nguồn:** Tài liệu `BaiTapNhom_Chuong1.pdf`, mục 2.1; thông tin nghiệp vụ do PM/BA cung cấp.
