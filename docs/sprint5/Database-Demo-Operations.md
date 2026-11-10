# Sprint 5 — Chuẩn bị database demo và hướng dẫn vận hành (SCRUM-114)

## Nền dữ liệu và minh chứng đã có

Các minh chứng sau đã tồn tại trên nhánh tích hợp `main` tại SHA `bf69ad0a59943ca554ef1b9740b3da954f2a8698`. Chúng thuộc phần WBS/Jira riêng được ghi trong [WBS-5.1.1-Handoff.md](../../docs/WBS-5.1.1-Handoff.md). Task này sử dụng lại các đầu vào đó, không nhận lại công việc seed ban đầu là phần mới do mình thực hiện.

| Minh chứng | Kết quả đã được ghi nhận |
|---|---|
| DemoSeed trên `BranchFlow_Dev` | Có log lần chạy đầu và chạy lại; fixture gồm bốn đơn hàng, sáu giao dịch kho, số dư của ba chi nhánh; thông tin fingerprint tài khoản không thay đổi |
| DemoSeed trên `BranchFlow_Test` | Có log lần chạy đầu và chạy lại; các assertion fixture giống nhau; tài khoản, mật khẩu và trạng thái tài khoản hiện có không thay đổi |
| Bộ kiểm tra API/SQL | Đăng nhập bốn vai trò; kiểm tra ba chi nhánh demo; phạm vi truy cập; snapshot đơn hàng; doanh thu ngày đối chiếu SQL; giá trị tồn kho |
| Minh chứng giao diện | Bốn ảnh chụp chọn lọc trên môi trường Test đã được commit trong `docs/evidence/sprint5-uat/` |
| Môi trường public | Không nằm trong các minh chứng này; log Test/Dev cục bộ không xác nhận URL public hoặc kiểm thử public |

Các file minh chứng chính xác:

- [Log DemoSeed Test lần đầu](../../tests/sprint5/DemoSeed-Test-2026-11-10.txt)
- [Log DemoSeed Test khi chạy lại](../../tests/sprint5/DemoSeed-Test-Rerun-2026-11-10.txt)
- [Log DemoSeed Dev lần đầu](../../tests/sprint5/DemoSeed-Dev-2026-11-10.txt)
- [Log DemoSeed Dev khi chạy lại](../../tests/sprint5/DemoSeed-Dev-Rerun-2026-11-10.txt)
- [Kết quả kiểm tra API/SQL](../../tests/sprint5/DemoApiResult-2026-11-10.txt)
- [Script điều phối seed demo WBS](../../tools/Seed-WbsDemo.ps1)
- [SQL fixture](../../database/005-wbs-demo-data.sql)
- [Script kiểm tra API/SQL](../../tests/sprint5/Verify-WbsDemoData.ps1)

## Phân tách database

| Database | Mục đích sử dụng | Thao tác được phép |
|---|---|---|
| `BranchFlow_Dev` | Dữ liệu phát triển cục bộ của thành viên | Phát triển local và nạp dữ liệu demo đã được duyệt |
| `BranchFlow_Test` | Fixture kiểm thử tích hợp SQL/API/QA | Chỉ dùng để kiểm thử và xác minh |
| Production / database trên máy chủ | Demo đã triển khai hoặc môi trường thực | Chỉ thao tác theo quy trình triển khai và sao lưu đã được duyệt; không dùng lệnh seed/reset local |

Không reset database cục bộ hiện có chỉ để làm cho một bài test chạy được. Script baseline tự kiểm tra database đích đang trống và từ chối chạy đè lên schema đã tồn tại.

## Trình tự chuẩn bị một database mới, dùng riêng cho kiểm thử

Chỉ thực hiện trình tự dưới đây khi database được tạo có chủ đích cho mục đích này và chưa chứa dữ liệu cần giữ lại.

1. Tạo database trống trên máy local với collation yêu cầu, theo hướng dẫn trong [database/README.md](../../database/README.md).
2. Chạy `database/001-baseline.sql` đúng một lần trên database trống đó.
3. Chạy `database/002-reference-data.sql`; script chỉ thêm các role, size và ingredient unit còn thiếu.
4. Chạy chương trình `tools/BranchFlow.DemoSeed` hiện có trong môi trường `Development` hoặc `Testing`. Một lượt seed thông thường chỉ tạo tài khoản/chi nhánh/phân công còn thiếu và không ghi đè mật khẩu đã tồn tại.
5. Chạy `tools/Seed-WbsDemo.ps1 -Database BranchFlow_Test` (chỉ chạy Dev sau khi đã kiểm tra Test) để thêm menu/đơn hàng/tồn kho demo WBS cục bộ. Wrapper từ chối server không phải local hoặc database khác hai tên được cho phép, so sánh fingerprint của chín tài khoản demo trước/sau và dừng nếu fixture hiện có xung đột thay vì ghi đè.
6. Chạy `tests/sprint5/Verify-WbsDemoData.ps1 -Database BranchFlow_Test` với API đang kết nối đúng database đó. Script xác minh bốn vai trò, chi nhánh, dữ liệu fixture, phạm vi API và giá trị SQL.

Không chạy `001-baseline.sql` trên database đã tồn tại. Không dùng `--reset-demo-passwords` trừ khi nhóm đã quyết định rõ việc đổi mật khẩu cả chín tài khoản demo và thông tin đăng nhập được phân phối qua kênh riêng tư đã thống nhất.

## Sao lưu và khôi phục

Sao lưu database Test dùng riêng trước khi thêm fixture lớn hoặc thay đổi dữ liệu demo. Tài khoản dịch vụ SQL Server cần có quyền ghi vào thư mục sao lưu. Mỗi lần sao lưu hãy dùng một tên file mới, duy nhất; không dùng lại tên file chung có thể ghi đè bản sao lưu của người khác.

```sql
BACKUP DATABASE [BranchFlow_Test]
TO DISK = N'<đường-dẫn-sao-lưu-local-duy-nhất>\BranchFlow_Test_<ngày-giờ>.bak'
WITH COPY_ONLY, CHECKSUM, STATS = 10;

RESTORE VERIFYONLY
FROM DISK = N'<đường-dẫn-sao-lưu-local-duy-nhất>\BranchFlow_Test_<ngày-giờ>.bak'
WITH CHECKSUM;
```

Để kiểm tra khả năng khôi phục mà không ghi đè database gốc:

1. Đọc tên logic của file dữ liệu và file log từ bản sao lưu bằng `RESTORE FILELISTONLY`.
2. Kiểm tra tên database scratch riêng và đường dẫn MDF/LDF mới chưa tồn tại.
3. Khôi phục vào database scratch mới bằng `WITH MOVE` cho cả hai file logic.
4. Chạy `DBCC CHECKDB` trên database scratch vừa khôi phục, sau đó chạy các smoke test cần thiết trên bản sao đó.
5. Chỉ xóa database scratch sau khi người phụ trách xác nhận việc kiểm tra khôi phục đã hoàn tất. Không drop `BranchFlow_Test`, `BranchFlow_Dev` hoặc database trên máy chủ trong quy trình này.

Ví dụ cấu trúc lệnh restore (thay tên logic và đường dẫn bằng giá trị lấy được từ `RESTORE FILELISTONLY`):

```sql
RESTORE FILELISTONLY
FROM DISK = N'<đường-dẫn-sao-lưu-local-duy-nhất>\BranchFlow_Test_<ngày-giờ>.bak';

RESTORE DATABASE [BranchFlow_Test_Restore]
FROM DISK = N'<đường-dẫn-sao-lưu-local-duy-nhất>\BranchFlow_Test_<ngày-giờ>.bak'
WITH
    MOVE N'<tên-logic-file-dữ-liệu>' TO N'<đường-dẫn-file-dữ-liệu-scratch-mới>\BranchFlow_Test_Restore.mdf',
    MOVE N'<tên-logic-file-log>' TO N'<đường-dẫn-file-log-scratch-mới>\BranchFlow_Test_Restore_log.ldf',
    RECOVERY, CHECKSUM, STATS = 10;

DBCC CHECKDB (N'BranchFlow_Test_Restore') WITH NO_INFOMSGS;
```

## Cấu hình và bảo vệ thông tin bí mật

- Lưu connection string trong User Secrets khi phát triển API local hoặc kho bí mật của nền tảng khi triển khai.
- Chỉ giữ mật khẩu demo trong biến môi trường của tiến trình khi script kiểm tra đang chạy: `BRANCHFLOW_DEMO_PASSWORD`.
- Không đưa mật khẩu demo, mật khẩu database, JWT key, access token, refresh token hoặc password hash vào Git, Jira, ảnh chụp terminal, log hoặc gói phát hành.
- Không chạy `dotnet user-secrets list` khi đang chụp minh chứng bàn giao.
- Không gửi connection string production vào trình chạy kiểm thử.

## Phạm vi giữ lại và dọn fixture

Fixture `BF-WBS5-v1` được thiết kế để chạy lặp lại mà không tạo bản ghi trùng. Script SQL của fixture **không tự động xóa** bốn đơn hàng demo, sáu giao dịch kho hoặc nguyên liệu demo sau khi kiểm tra.

Đây là chủ ý vì các bản ghi đó có thể tiếp tục được QA/UAT và nhóm dùng để demo. Không xóa chúng hoặc reset database nếu chưa có quyết định rõ ràng của Khang và bản sao lưu đã được xác minh. Runner API quản trị trong nhánh kiểm thử SCRUM-110/112 tạo các bản ghi `QA110*` riêng và chỉ dọn các bản ghi đó trong khối `finally`; runner không dọn fixture WBS dùng chung.

## Danh sách bàn giao

| Hạng mục | Minh chứng / trạng thái |
|---|---|
| Hướng dẫn schema và seed dữ liệu tham chiếu | Đã có trong `database/README.md` |
| Seed Dev và Test hiện có | Đã có log commit; xem các liên kết ở trên |
| Fixture demo WBS có thể chạy lặp lại | Đã có; log lần đầu và chạy lại cho thấy không tạo dữ liệu trùng |
| Đối chiếu API/SQL | Đã có file kết quả |
| Quy trình backup/restore | Đã được ghi trong tài liệu này; người phụ trách database vẫn cần thực hiện restore drill |
| Thông tin bí mật không đưa vào repository | Đã có hướng dẫn; vẫn cần xác nhận trên môi trường triển khai thật |
| Xác minh database production/public | Chưa chạy; không được suy ra từ kết quả Dev/Test local |
| Dọn fixture WBS dùng chung | Tạm hoãn đến khi Khang xác nhận không còn cần dùng |

Giữ SCRUM-114 mở cho đến khi Khang nhận tài liệu, xác nhận thời gian giữ fixture và việc restore drill (nếu được yêu cầu) đã thực hiện, có kết quả ghi nhận.
