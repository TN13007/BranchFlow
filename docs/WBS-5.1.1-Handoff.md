# WBS-5.1.1 — Chuẩn bị mẫu 3 chi nhánh và 4 vai trò

- Người thực hiện: Công Thành, có công cụ hỗ trợ; ngày thực hiện/kiểm chứng: 10/11/2026.
- Jira được PM map: SCRUM-127, mục Lịch / effort / WBS có 5.1.1. Story cha: SCRUM-107.
- Branch bàn giao: feat/SCRUM-127-wbs-5.1.1-handoff; nền 47541ee. SHA bàn giao tra bằng git log -1 trên nhánh này; comment Jira ghi link commit cụ thể. Không gọi SHA nền là SHA đã chứa phần bổ sung.
- Môi trường: SQL Server localhost, BranchFlow_Test / BranchFlow_Dev; API/Web .NET 10 Release. Kiểm chứng UI bốn vai trò trong gói này thuộc Test, không suy thành public hoặc UI Dev đã nghiệm thu.

## Phần có sẵn và phần em thực hiện

Core có DemoSeed ba chi nhánh BF-DEMO-01/02/03 và chín tài khoản bao phủ bốn vai trò. Em tiếp nhận, rà soát và giữ nguyên tài khoản/mật khẩu/trạng thái, không nhận là tự tạo lại core. Em bổ sung wrapper Seed-WbsDemo.ps1, fixture 005-wbs-demo-data.sql, script kiểm tra API/SQL và tài liệu bàn giao để dùng chung cho các WBS 5.1.1–5.1.3. Script 003-demo-menu.sql là đầu vào có sẵn, được kèm để wrapper chạy được. Bộ 62 món tùy chọn không áp dụng trong lượt này.

## Tài khoản và expected

| Vai trò / trường hợp | Tài khoản | Phạm vi kỳ vọng |
|---|---|---|
| Owner | demo.owner | Xem phạm vi được cấp; không có thao tác ghi trong màn đã kiểm |
| Admin Tổng | demo.systemadmin | Quản trị theo vai trò, đọc ba chi nhánh demo |
| Admin CN | demo.branchadmin.01/02/03 | Mỗi tài khoản gắn đúng chi nhánh tương ứng |
| Employee | demo.employee.a / demo.employee.b | A thuộc 01, B thuộc 02 |
| Kiểm tra trạng thái | demo.employee.locked / demo.employee.unassigned | Giữ trạng thái khóa / chưa phân công |

Không kèm mật khẩu, token hoặc hash. Khang/QA dùng thông tin đăng nhập demo đã được nhóm cấp.

## Các bước kiểm chứng và actual

1. Đọc seed hiện có, kiểm tra chín tài khoản và ba chi nhánh. Dùng wrapper so sánh fingerprint tài khoản trước/sau trong bộ nhớ, không xuất hash.
2. Chạy wrapper trên Test trước, sau đó Dev; chạy lại trên mỗi DB. Expected: không đổi tài khoản và không nhân đôi fixture. Actual: log Pass trên cả hai DB, ID/mã giữ nguyên, bốn đơn fixture và sáu phiếu giữ nguyên — Pass.
3. Chạy Verify-WbsDemoData.ps1 với API kết nối đúng DB. Expected: đăng nhập trả đúng bốn role, API đọc ba chi nhánh, Admin CN01 chỉ đọc phạm vi 01. Actual: các assertion tương ứng trong DemoApiResult-2026-11-10.txt đều Pass. Không coi log tổng hợp này là kiểm hết mọi quyền âm.
4. Đăng nhập bốn vai trò trên Web Test, đối chiếu màn theo vai trò. Actual: có lượt kiểm và ảnh UI thật trong báo cáo UAT; ảnh chọn lọc trong thư mục evidence của gói. Đây là tự kiểm tra, cần QA retest độc lập.

## Thành phần trong commit và gói ZIP đã gửi

- tools/Seed-WbsDemo.ps1, database/003-demo-menu.sql và database/005-wbs-demo-data.sql.
- tests/sprint5/Verify-WbsDemoData.ps1 và năm log DemoSeed/DemoApi (lần đầu, chạy lại Test/Dev).
- Bản tài liệu này là báo cáo riêng WBS 5.1.1. Các báo cáo Sprint5-Demo-Data/UAT/Retest rộng hơn có trong ZIP đã gửi Jira, không đưa vào commit này để tránh lẫn bàn giao WBS khác.
- Bốn ảnh Test được commit tại docs/evidence/sprint5-uat/: owner-revenue-1366.png, system-employees-768.png, branch-revenue-1366.png và employee-history-1366.png. Trong ZIP cũ chúng nằm ở evidence/. Ảnh giữ số liệu lúc chụp, không dùng như tổng doanh thu hiện tại.

## Nhận gói và chạy lại

Checkout nhánh bàn giao hoặc giải nén ZIP vào thư mục riêng, giữ cấu trúc. DB phải có baseline/reference/DemoSeed của core trước; không chạy lại baseline hay reset DB đang có. Từ root gói chạy `./tools/Seed-WbsDemo.ps1 -Database BranchFlow_Test`. Chỉ chạy Dev sau khi kiểm Test. Wrapper không tạo user mới hoặc đổi mật khẩu. API/SQL verifier cần môi trường BRANCHFLOW_DEMO_PASSWORD được người chạy cấp riêng và API kết nối đúng DB; không gửi secret vào Jira.

## Tồn đọng và bàn giao

Em bàn giao dữ liệu và luồng demo cho Thuận/Khang. Chờ Khang review, QA retest và tổng duyệt nhóm; public chưa chạy. Commit có issue key SCRUM-127 cung cấp đúng script và minh chứng liên quan; ZIP đã gửi trước commit vẫn là bản file tại thời điểm gửi. Chưa merge vào main, chưa có PR/review được chấp nhận. Các thay đổi FE/CSS, clock integration test, slide và tài liệu WBS khác nằm ngoài commit này. Script 005 và verifier dùng chung 5.1.1–5.1.3 được kèm vì wrapper cần chúng; không coi việc kèm file là đã nghiệm thu thêm các WBS đó. Hồ sơ Owner chỉ đọc còn phụ thuộc API/quyền ở WBS 3.2.2.5, không dùng task này để xác nhận phần đó đạt. Chưa ghi actual giờ hoặc tự chốt Done cho SCRUM-127/Story.
