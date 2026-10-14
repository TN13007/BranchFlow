# Week 1 Summary – BranchFlow (bản tổng hợp dự thảo)

**Thời gian kế hoạch:** 12–18/10/2026.  
**Thời điểm biên soạn ban đầu:** 14/10/2026; **cần cập nhật kết quả thực tế khi kết thúc tuần**.  
**Nguồn thông tin:** Xác nhận của PM và trạng thái repository/Jira được cung cấp; không tự coi kế hoạch là kết quả đã hoàn thành.

## Tổng kết (300–500 từ)

Trong giai đoạn khởi động, Nhóm 6 lựa chọn BranchFlow – hệ thống quản lý chuỗi cửa hàng đồ uống – làm đề tài chính và đối chiếu với phương án website bán hàng cho hộ kinh doanh theo sáu tiêu chí của giảng viên. Nhóm có năm thành viên, gồm PM kiêm BA và hỗ trợ lập trình, ba UI/UX Designer kiêm Developer và một QA/Documentation hỗ trợ lập trình. PM xác nhận các thành viên đã thống nhất vai trò, trách nhiệm và thời gian tham gia 12–15 giờ mỗi tuần. Đề tài vẫn đang chờ giảng viên phê duyệt chính thức.

Về cơ sở nghiệp vụ, nhóm sử dụng thông tin từ hai năm làm việc của thành viên BA tại Sâm thảo mộc Tuấn Kiệt. Những hoạt động đã được mô tả gồm nhận order bằng lời nói rồi nhập POS, ghi sổ doanh thu cuối ca, kiểm đếm kho và thông báo thiếu hàng qua tin nhắn. Việc xem thông tin bảng lương trước kỳ trả cũng còn hạn chế theo trải nghiệm của BA. Đây là nguồn quan sát trong quá khứ, chưa phải kết quả khảo sát độc lập; nhóm chưa có căn cứ để khẳng định quy trình hiện nay không thay đổi hoặc đo lường tỷ lệ sai sót.

Nhóm đã thống nhất mục tiêu xây dựng website demo trong 50 ngày với bảy phân hệ, hỗ trợ ba vai trò Owner, Admin và Employee. Phạm vi sử dụng dữ liệu giả lập và mô hình nhiều chi nhánh, không triển khai vận hành tại cửa hàng thật. Công nghệ dự kiến là ASP.NET Core, Blazor, Bootstrap và SQL Server. Về công cụ, Jira Space và Sprint 1 đã được tạo; PM xác nhận đã thiết lập mười task khởi động. Repository GitHub BranchFlow đã được khởi tạo và có cấu trúc thư mục ban đầu. Tuy nhiên, nội dung README/tài liệu và các đóng góp thực chất của từng thành viên vẫn cần rà soát theo lịch sử commit trước khi báo hoàn tất.

Theo thông tin PM cung cấp, nhóm chưa ghi nhận bất đồng đáng kể về mục tiêu hay phạm vi và chưa có thành viên báo khó khăn cụ thể. Những giả định còn mở liên quan đến tính đại diện của quy trình cửa hàng, khả năng hoàn thành bảy phân hệ trong 50 ngày, dữ liệu demo và giới hạn hosting miễn phí. Trong Tuần 2, nhóm dự kiến xin duyệt đề tài, chuẩn hóa yêu cầu, lập WBS/Gantt, thiết kế luồng nghiệp vụ và giao diện, đồng thời kiểm tra phương án triển khai demo. Các nội dung chưa có bằng chứng sẽ chỉ cập nhật là hoàn thành khi có tài liệu hoặc kết quả thực tế.

## Đối chiếu 6 câu hỏi của giảng viên

1. **Đã làm gì?** Chọn đề tài nội bộ, chốt mục tiêu/phạm vi/nhân lực; lập dự thảo hồ sơ; thiết lập Jira và repository ban đầu.
2. **Đã kiểm chứng gì?** Có cơ sở quan sát từ thành viên BA; **chưa** phỏng vấn độc lập hoặc đo định lượng.
3. **Giả định còn lại?** Hiện trạng hoạt động của cửa hàng, khả năng đáp ứng phạm vi/hosting/dữ liệu demo.
4. **Có bất đồng không?** Theo PM, **chưa ghi nhận**.
5. **Có thành viên gặp khó khăn?** Theo PM, **chưa ghi nhận**.
6. **Tuần 2 làm gì?** Phê duyệt đề tài, đặc tả yêu cầu, Use Case, ERD/UI, WBS/Gantt và chuẩn bị Sprint kế tiếp.

## Mục phải kiểm tra lại trước khi nộp

- [ ] Xác nhận lại kết quả thực hiện thật sự đến hết Sprint 1 (18/10).
- [ ] Cập nhật chính xác trạng thái 10 task trên Jira.
- [ ] Kiểm tra đủ đóng góp thực chất của 5 thành viên trên GitHub.
- [ ] Cập nhật thông tin duyệt đề tài/lịch PCG nếu có thay đổi.
