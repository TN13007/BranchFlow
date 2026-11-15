# WBS-5.4.3 — Risk and Handover

> **Nguồn dữ liệu:** Báo cáo Sprint 5, WBS_Jira_Sprint5.csv, SoSanh_KeHoach_ThucTe.csv, PhanBo_NguonLuc_Sprint5.csv và PM_Review.md.
>
> **Mốc trạng thái của bảng:** 12/11/2026.
>
> **Lưu ý:** Đây là bản tổng hợp rủi ro và bàn giao tại mốc rà soát, không phải kết quả retrospective cuối Sprint.

## 1. Mục tiêu

Tổng hợp các rủi ro, vấn đề còn tồn đọng và điều kiện cần hoàn thành trước khi nghiệm thu, triển khai và bàn giao BranchFlow MVP. Nội dung được xây dựng dựa trên trạng thái Jira, kế hoạch ProjectLibre, kết quả kiểm thử và nhận xét PM.

Mục tiêu chính:
- Xác định các hạng mục chưa đạt đầy đủ Acceptance Criteria.
- Theo dõi các phụ thuộc có thể ảnh hưởng đến nghiệm thu và triển khai.
- Xác định người hoặc nhóm phụ trách xử lý tiếp.
- Bảo đảm các đầu ra, bằng chứng kiểm thử và tài liệu được bàn giao có thể truy vết.

## 2. Tình hình tại thời điểm rà soát

Sprint 5 có 30 subtask, mỗi task được estimate 2 giờ, tổng kế hoạch 60 giờ.

| Trạng thái | Số task | Tỷ lệ |
|---|---:|---:|
| Done | 14 | 46,7% |
| In Progress | 8 | 26,7% |
| In Review | 5 | 16,7% |
| To Do | 3 | 10% |
| **Tổng** | **30** | **100%** |

Sáu Story cha vẫn ở trạng thái In Progress tại thời điểm báo cáo. Vì vậy, việc có các subtask Done không đồng nghĩa toàn bộ Story đã đủ điều kiện nghiệm thu.

Phân bổ nguồn lực dự kiến là 12 giờ/người cho 5 thành viên. Tuy nhiên, trường Actual h trong bảng phân bổ chưa được xác nhận. Do đó, chưa thể kết luận chênh lệch effort thực tế giữa kế hoạch và kết quả.

## 3. Risk Register

| ID | Rủi ro / vấn đề | Bằng chứng và ảnh hưởng | Phụ trách đề xuất | Hành động xử lý |
|---|---|---|---|---|
| R-01 | Quản trị catalog chưa được xác nhận đầy đủ | SCRUM-116 còn In Progress; PM yêu cầu kiểm chứng mutation, invalid và duplicate | Trường Tài, QA | Hoàn thiện CRUD menu, topping, giá; kiểm tra dữ liệu không hợp lệ, trùng tên và quyền truy cập |
| R-02 | Giao diện quản trị menu chưa hoàn tất review | SCRUM-123 đang In Review | Công Thành, PM | Đối chiếu UI với API thật, kiểm tra loading, empty, error và thao tác bật/tắt |
| R-03 | Kiểm thử menu chưa hoàn thành | SCRUM-129 đang In Progress | Thuận, QA | Kiểm thử quyền, giá, trạng thái ngừng bán và dữ liệu không hợp lệ |
| R-04 | Hồi quy xác thực chưa được xác nhận đầy đủ | PM Review yêu cầu kiểm tra 401/relogin | QA, Backend | Kiểm tra phiên hết hạn, đăng nhập lại và trạng thái thao tác sau lỗi xác thực |
| R-05 | Fault-injection cho thao tác nhập kho còn thiếu | PM Review nêu yêu cầu fault-inject POST kho | QA, Backend | Kiểm tra lỗi trong lúc gửi, phản hồi không rõ ràng và nguy cơ ghi giao dịch trùng |
| R-06 | UAT độc lập chưa hoàn tất | SCRUM-131 In Progress; SCRUM-132 In Review | QA, PM | Chạy UAT theo bốn vai trò, ghi nhận defect và tổng hợp kết quả |
| R-07 | Public HTTPS và smoke test chưa hoàn thành | SCRUM-115 In Progress; SCRUM-133 và SCRUM-138 To Do | Khang, Thuần, QA | Chốt cloud/URL, triển khai candidate đã nghiệm thu và chạy smoke test public |
| R-08 | Gói Release hiện tại chưa được xác nhận là candidate cuối | PM Review ghi nhận gói Release đang lưu được tạo từ dbfa03d | Thuần, Khang | Publish lại từ candidate đã nghiệm thu, kiểm tra cấu hình và secrets |
| R-09 | Dữ liệu Actual effort chưa đầy đủ | CSV nguồn lực ghi Chưa xác nhận cho cả 5 thành viên | PM và các thành viên | Thu thập giờ thực tế có xác nhận; không dùng estimate hoặc giờ phân công thay cho Actual |
| R-10 | Phân bổ công việc có ngày vượt giới hạn nguồn lực | ProjectLibre ghi nhận 14 cặp người-ngày vượt 2 giờ, cao nhất 4 giờ | PM | Rà lại assignment, ưu tiên công việc chặn demo và xác nhận remaining estimate |

## 4. Công việc còn mở và hướng bàn giao

### 4.1 Backend và quản trị menu

- SCRUM-116 — Trường Tài: Hoàn thiện quản trị menu và bật/tắt chi nhánh.
- Phạm vi WBS liên quan: 3.3.1.2, 3.3.1.3, 3.3.1.4 và 3.3.2.2.
- Cần bàn giao API, quy tắc kiểm tra dữ liệu, phân quyền và kết quả kiểm thử.
- Các thay đổi giá phải chỉ ảnh hưởng đơn mới; đơn đã tạo phải giữ snapshot giá, size và topping.

### 4.2 Frontend

- SCRUM-123 — Công Thành: Hoàn thiện giao diện quản trị menu.
- Cần xác nhận giao diện sử dụng API thật và xử lý đầy đủ trạng thái tải, dữ liệu rỗng, lỗi và thao tác ngừng bán.
- Cần phối hợp Backend để thống nhất hợp đồng request/response.

### 4.3 QA và nghiệm thu

- SCRUM-129: Kiểm thử menu và sửa/hủy đơn.
- SCRUM-131: Chạy UAT local và ghi defect.
- SCRUM-132: Retest bản Release và Test Summary.
- SCRUM-133: Smoke public và kiểm tra minh chứng.

Các kết quả kiểm thử cần gắn với phiên bản ứng dụng, môi trường, dữ liệu và bằng chứng tương ứng. Không dùng kết quả test của một phiên bản để kết luận cho candidate khác nếu chưa xác nhận mã nguồn và môi trường tương đương.

### 4.4 Release và triển khai

- SCRUM-115: Phối hợp deploy và tổng hợp minh chứng public.
- SCRUM-138: Triển khai demo thực tế và bàn giao URL.
- Khang chốt tài nguyên cloud và URL triển khai.
- Thuần phối hợp gói API, Web và DB demo.
- QA thực hiện smoke test sau khi URL public sẵn sàng.

Chỉ xác nhận hoàn tất bàn giao sau khi candidate được nghiệm thu, cấu hình triển khai được kiểm tra và các luồng ưu tiên hoạt động trên môi trường public.

## 5. Điều kiện bàn giao

| Hạng mục | Điều kiện cần xác nhận | Trạng thái tại mốc rà soát |
|---|---|---|
| Backend catalog | Mutation, invalid, duplicate và phân quyền | Còn cần QA xác nhận |
| Frontend menu | API thật, trạng thái loading/empty/error, bật/tắt | Đang review |
| Đơn hàng | Giữ snapshot giá và lịch sử sửa/hủy | Có kết quả kiểm chứng trong báo cáo; tiếp tục hồi quy |
| Kho | Nhập/xuất, lịch sử và fault-injection | Còn kiểm tra bổ sung |
| Xác thực | 401 và relogin | Còn cần hồi quy |
| UAT | UAT độc lập và Test Summary cuối | Chưa nghiệm thu |
| Release | Publish từ candidate đã nghiệm thu | Cần publish lại |
| Public demo | HTTPS, URL và smoke test | Chưa nghiệm thu |
| Tài liệu | User Guide, cấu hình, DB demo, nguồn minh chứng | Cần kiểm tra gói cuối |

## 6. Nguyên tắc bàn giao và truy vết

Mỗi hạng mục được bàn giao cần có:

1. Mã WBS và Jira liên quan.
2. Người phụ trách và người tiếp nhận.
3. Trạng thái công việc tại thời điểm bàn giao.
4. PR hoặc commit chứa thay đổi.
5. Kết quả build và test tương ứng.
6. Các lỗi hoặc điều kiện còn mở.
7. Remaining estimate được thành viên xác nhận nếu công việc tiếp tục.

Không chuyển trạng thái Done chỉ dựa trên việc code đã được merge hoặc build thành công. Cần đối chiếu Acceptance Criteria, bằng chứng kiểm thử và review của người có trách nhiệm.

## 7. Các giới hạn của dữ liệu

- Trạng thái Jira trong tài liệu là snapshot ngày 12/11/2026.
- Actual Start, Actual Finish và Actual Work chưa có timesheet được xác nhận đầy đủ.
- Các số giờ trong bảng phân bổ là estimate theo kế hoạch, không phải giờ thực tế.
- Việc vượt 2 giờ/ngày trong ProjectLibre là vượt mức phân công kế hoạch, không chứng minh thành viên đã làm vượt thời gian thực tế.
- UAT độc lập, Test Summary cuối và public HTTPS chưa được xác nhận nghiệm thu tại thời điểm PM Review.
- Nội dung này không thay thế biên bản retrospective hoặc biên bản nghiệm thu cuối Sprint.

## 8. Nguồn tham chiếu

- `WBS_Jira_Sprint5.csv`
- `SoSanh_KeHoach_ThucTe.csv`
- `PhanBo_NguonLuc_Sprint5.csv`
- `PM_Review.md`
- `NHOM6_BaoCao_Tuan5.docx`
- PR tích hợp: https://github.com/TN13007/BranchFlow/pull/65
- Commit tích hợp: https://github.com/TN13007/BranchFlow/commit/0e0c51431ded1ce77e292536f506755a0e6c258a
- User Guide: https://github.com/TN13007/BranchFlow/blob/0e0c514/docs/Sprint5-User-Guide.md
- QA Retest 12/11: [Kết quả retest trên 0e0c514](../tests/sprint5/QA-Retest-2026-11-12.md)

## 9. Tình trạng khi đóng Sprint 5

- Mốc kế hoạch là 13/11/2026 lúc 23:00 (giờ Việt Nam). Bản release Azure được triển khai ngày 14/11 từ SHA `28441013318ffee1a6b8541f1318dab9ca124201`. R-07 và R-08 đã có minh chứng phát hành và 27 kiểm tra HTTP public; kết quả và phạm vi được ghi trong [hồ sơ triển khai](sprint5/Public-Smoke-Handoff.md).
- Tại thời điểm đóng Sprint 5 ngày 15/11, Jira có 20/30 subtask Done. Số này phản ánh lúc đóng sprint. Các thay đổi ghi nhận sau ngày 13/11 cần được đối chiếu riêng khi báo cáo tiến độ.
- Sprint 6 tiếp nhận phần việc còn lại của SCRUM-101, SCRUM-102, SCRUM-103, SCRUM-105 và SCRUM-107. UAT ba người và xác nhận bàn giao theo dõi ở SCRUM-131 và SCRUM-139.
