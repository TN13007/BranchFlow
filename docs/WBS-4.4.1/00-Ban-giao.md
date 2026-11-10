# WBS-4.4.1 — Bàn giao kịch bản UAT bốn vai trò

Em là Công Thành, lập gói ngày10/11/2026. Nhánh riêng **docs/SCRUM-131-wbs-4.4.1-thanh-uat**, nền47541ee. Đây là commit tài liệu/minh chứng, không sửa ứng dụng, không đưa code responsive/kho/doanh thu hoặc test của task khác vào.

## Đọc theo thứ tự

1. [Kịch bản / expected / điều kiện chấp nhận](01-Kich-ban.md).
2. [Actual local theo case / người / ngày / candidate](02-Ket-qua-local.md).
3. [Phản hồi và tồn đọng](03-Phan-hoi-ton-dong.md).
4. [Ghi nhận thực tế và biên bản chờ xác nhận](04-Bien-ban-va-xac-nhan.md).
5. [Comment bàn giao theo mẫu](05-Comment-Jira.txt).

## Mapping và người nhận

Jira **SCRUM-131**, mục Lịch / effort / WBS hiện ghi4.4.1;4.4.3;4.4.4;4.6.4, StorySCRUM-107. Assignee là Nguyễn Thuận. Đây là **đóng góp/bàn giao của Thành cho Thuận** theo WBS4.4.1, không đổi assignee, không thực hiện thay QA/người dùng hoặc tự đóng issue. Đầu ra tiếp theo4.4.3 giao Hòa Thuận theo file cá nhân; Khang review.

## Nền có sẵn / em thực hiện / em kiểm

Core có Web/API/DB và tài khoản/menu/các luồng MVP. Em tiếp nhận kịch bản và các lượt tự kiểm đã có, chuẩn hóa thành bốn phần tách biệt, cập nhật U25 từ NotRun lịch sử thành Pass theo retest và bổ sung U27/U28. Em rà tính nhất quán ảnh/log/case và chạy lại SQL **chỉ đọc** để đối chiếu dữ liệu giữ lại; không phát lại các thao tác ghi UI trong lượt đóng gói này.

Ảnh/actual UI cũ ngày10/11 trên47541ee+working tree; không gọi SHA commit tài liệu này là SHA ứng dụng đã được chạy UAT. Các ảnh do Thành thao tác với công cụ hỗ trợ, không phải ảnh dựng. Không có xác nhận QA/PM/người dùng được tạo thay người khác.

## Phạm vi minh chứng

- Ảnh chọn lọc trong `evidence/`, bản trích nguồn và log trong `evidence/records/`; mỗi case chỉ khẳng định điều quan sát được, không dùng một ảnh để nhận mọi biến thể đã đạt.
- `Verify-UatEvidence.sql` chỉ đọc trên BranchFlow_Test. Dùng cùng DB giữ fixture; script không seed/reset hoặc gửi lại đơn.
- Tổng tiền ảnh được đọc theo thời điểm:128.020,50đ trước/sau hủy ID24 ở lượtL01;215.020,50đ sau ba đơn retest ởL02. Không dùng chúng làm tổng hiện tại; SQL đối chiếu mới có snapshot riêng.
- Chưa có URL/deployedSHA public. Chưa có UAT ba người dùng, reviewPM hoặc QA độc lập trong gói Thành. U26 Owner đọc hồ sơ vẫn chờ chốt contract/quyền.

Kịch bản28ca local, kết quả27Pass/1Blocked là số kịch bản, không cộng với số assertion HTTP/unit/SQL thành số test duy nhất. Đây là phần Thành tự kiểm và bàn giao; nghiệm thu nhóm/public còn chờ đúng đầu vào và người thực hiện.
