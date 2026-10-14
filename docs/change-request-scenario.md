# Bài tập 9 – Tình huống thay đổi yêu cầu trong Tuần 1

**Bối cảnh giả định từ giảng viên:** Khách hàng yêu cầu bổ sung ứng dụng di động, chatbot AI và chương trình tích điểm, nhưng **không tăng ngân sách và thời hạn**. Đây là bài tập xử lý thay đổi, **không phải yêu cầu thật của cửa hàng Tuấn Kiệt**.

## Phân tích tình huống (300–500 từ)

Sau khi nhóm thống nhất phát triển website, yêu cầu bổ sung ứng dụng di động, chatbot AI và chương trình tích điểm sẽ làm phạm vi dự án tăng đáng kể. Nhóm phải thiết kế thêm trải nghiệm trên nền tảng khác, xác định dữ liệu khách hàng, quy tắc tích điểm, hội thoại chatbot và cơ chế tích hợp với hệ thống hiện tại. Khối lượng phát triển, kiểm thử và tài liệu đều tăng; rủi ro bảo mật, đồng bộ dữ liệu và lỗi sản phẩm cũng cao hơn. Khi ngân sách và thời hạn giữ nguyên, sự gia tăng phạm vi có thể buộc nhóm cắt giảm tính năng đã cam kết hoặc làm giảm chất lượng.

Vì vậy, nhóm không nên chấp thuận toàn bộ yêu cầu ngay. PM cần ghi nhận thành Change Request, làm rõ mục đích kinh doanh, người sử dụng, mức độ ưu tiên và tiêu chí nghiệm thu của từng tính năng. Đặc biệt, cần hỏi liệu người dùng thật sự cần ứng dụng Android/iOS riêng hay website responsive đã đáp ứng; chatbot giải quyết tình huống nào và dùng nguồn dữ liệu gì; chương trình tích điểm áp dụng quy tắc, kỳ hạn và bảo vệ dữ liệu cá nhân ra sao.

Trước khi quyết định, nhóm phải ước lượng công sức, năng lực thành viên, thời gian còn lại, chi phí nền tảng AI, thay đổi cơ sở dữ liệu, khối lượng kiểm thử và các phụ thuộc kỹ thuật. PM trình các phương án để nhóm và người có thẩm quyền cân nhắc trên cùng một cơ sở thông tin. Với điều kiện đã cho, lựa chọn hợp lý là **giữ MVP website** và sử dụng giao diện responsive để hỗ trợ điện thoại; đưa ứng dụng native, chatbot và tích điểm vào backlog giai đoạn sau. Nếu một yêu cầu mới có giá trị cao hơn, nhóm có thể đề xuất **thay thế** một hạng mục ít ưu tiên, hoặc đàm phán điều chỉnh thời hạn/nguồn lực; không âm thầm thêm công việc.

Khi thay đổi được chấp thuận, cần cập nhật Change Request Log, phạm vi/Project Charter, yêu cầu và tiêu chí nghiệm thu, WBS, Gantt, Sprint Backlog, phân công, rủi ro, kế hoạch kiểm thử và lịch bàn giao. Trong trường hợp BranchFlow, do đây là demo học thuật, quyết định thay đổi phải phù hợp cơ chế phê duyệt của nhóm, PCG và giảng viên. Nguyên tắc xuyên suốt là minh bạch tác động và chỉ cam kết những gì có thể kiểm tra được.

## Đối chiếu 5 câu hỏi

| Câu hỏi | Kết luận |
|---|---|
| 1. Ảnh hưởng phạm vi? | Tăng chức năng, nền tảng, dữ liệu, thiết kế và kiểm thử; tác động thời gian, chi phí, nguồn lực, chất lượng, rủi ro. |
| 2. Có nên đồng ý ngay? | **Không**; phải đánh giá và được phê duyệt phù hợp. |
| 3. Cần kiểm tra gì? | Giá trị, ưu tiên, effort, năng lực, thời gian, kiến trúc, chi phí, rủi ro, test và dữ liệu cá nhân. |
| 4. Nếu không làm đủ? | Giữ MVP; responsive web thay một phần native; chuyển các mục khác sang backlog hoặc thương lượng đổi phạm vi. |
| 5. Cập nhật tài liệu nào? | Change Request, Charter/Scope, Requirements, WBS/Gantt, Sprint Plan/Backlog, Risk Register, Test Plan và tiêu chí nghiệm thu. |

**Nguồn:** `BaiTapNhom_Chuong1.pdf`, phần G – Bài tập 9.
