# Stakeholder Register – BranchFlow

**Nhóm 6** | **Phiên bản:** 1.0 | **Trạng thái:** Xác định sơ bộ để khởi động dự án.

## 1. Danh sách các bên liên quan

| Stakeholder | Vai trò/quan hệ với dự án | Nhu cầu hoặc kỳ vọng | Ảnh hưởng | Cách phối hợp |
|---|---|---|---|---|
| Giảng viên học phần | Hướng dẫn, đánh giá và phê duyệt đề tài theo quy định | Mục tiêu rõ, phạm vi khả thi, minh chứng trung thực | Cao | Trình đề tài; báo cáo, xin ý kiến ở các mốc phê duyệt. |
| PCG (nhóm kiểm soát) | Giám sát kế hoạch, rủi ro và tiến độ | Sprint Plan khả thi, đúng ưu tiên, có minh chứng | Cao | Gửi Sprint Plan trước họp 1 ngày; cấp quyền xem Jira/GitHub; nhận phản hồi. |
| Bá Thành Chí Khang – PM/BA | Điều phối dự án, đầu mối nghiệp vụ | Hoàn thành hồ sơ, kiểm soát phạm vi và tiến độ | Cao | Jira, họp nhóm, quản lý quyết định/rủi ro và thống nhất yêu cầu. |
| 3 UI/UX Designer – Developer | Thiết kế, triển khai chức năng | Yêu cầu rõ, môi trường tích hợp ổn định, task có hạn | Cao | Jira, GitHub, review giao diện/mã nguồn, phối hợp theo module. |
| QA / Documentation | Kiểm thử, đảm bảo chất lượng tài liệu | Có tiêu chí nghiệm thu, phiên bản có thể test, đủ thời gian rà soát | Trung bình–cao | Test case, bug tracker, checklist tài liệu và báo cáo. |
| Owner (người dùng mô phỏng) | Xem dữ liệu toàn chi nhánh | Dashboard mobile, quyền chỉ xem, số liệu rõ ràng | Trung bình | Kiểm thử kịch bản theo role Owner bằng dữ liệu giả lập. |
| Admin (người dùng mô phỏng) | Quản lý vận hành chi nhánh | Điều chỉnh menu, nhân viên, order sai, kho và bảng lương | Trung bình | UAT nội bộ bằng kịch bản được BA duyệt. |
| Employee (người dùng mô phỏng) | Bán hàng và thao tác kho | Tạo order nhanh trên mobile, xem đúng lương cá nhân | Trung bình | Demo và kiểm thử thao tác mobile. |
| Chủ/quản lý Tuấn Kiệt | Bối cảnh tham khảo, **chưa là đối tác chính thức** | Không bị gán nghĩa vụ cung cấp dữ liệu hoặc nghiệm thu | Chưa xác định | Chỉ trao đổi khi có sự đồng ý; không ghi là sponsor/khách hàng. |

**Giới hạn:** Các vai trò Owner/Admin/Employee của hệ thống là **persona demo**, không đồng nghĩa với việc những người thật tại Tuấn Kiệt đã tham gia dự án hoặc xác nhận yêu cầu.

## 2. Bốn câu hỏi phân tích bắt buộc

### 2.1. Ai có quyền quyết định phạm vi dự án?

PM tổng hợp yêu cầu và đề xuất; các thành viên thảo luận để thống nhất trong quyền hạn của nhóm. Sprint Plan được PCG xem xét theo quy định. Những thay đổi lớn hoặc đề tài ngoài danh sách gợi ý phải xin ý kiến giảng viên theo cơ chế học phần. Sponsor chính thức hiện **chưa được xác nhận**.

### 2.2. Ai dùng sản phẩm nhưng không trực tiếp trả tiền?

Các tài khoản **Owner, Admin, Employee** được tạo để mô phỏng sử dụng website demo. Không có khách hàng trả phí và không thu phí sử dụng hệ thống trong học phần.

### 2.3. Ai có thể làm dự án chậm tiến độ?

- PM/BA: chậm chốt yêu cầu, giao việc hoặc thay đổi phạm vi.
- Developer: chậm hoàn thiện hoặc tích hợp chức năng.
- QA: chậm chuẩn bị test case hoặc phát hiện lỗi muộn.
- PCG/giảng viên: cần thêm thời gian phản biện, phê duyệt theo quy trình.
- Hạ tầng: hạn chế dịch vụ miễn phí ảnh hưởng deploy/demo.

Điểm này phân tích **nguy cơ**, không quy trách nhiệm thực tế cho bất kỳ cá nhân nào.

### 2.4. Xử lý thế nào khi yêu cầu “khách hàng” và người dùng khác nhau?

Ghi nhận riêng từng yêu cầu, xác định mục tiêu và quyền lợi liên quan, đánh giá tác động đến MVP/nguồn lực/thời hạn. PM điều phối thảo luận ưu tiên theo giá trị và tính khả thi; nếu làm thay đổi phạm vi đã duyệt phải lập Change Request, cập nhật tài liệu và trình đúng người có thẩm quyền. Trong BranchFlow, “khách hàng” chủ yếu là tình huống giả định của bài tập, không phải bên đặt hàng thật.

## 3. Theo dõi cập nhật stakeholder

- [ ] Xác nhận Project Sponsor chính thức hoặc cơ chế ủy quyền nếu lớp phân công.
- [ ] Cập nhật thông tin người kiểm soát PCG và kênh liên hệ khi được cấp.
- [ ] Cập nhật danh sách người nghiệm thu demo nội bộ khi kế hoạch nghiệm thu được chốt.
