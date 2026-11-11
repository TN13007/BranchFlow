# WBS-5.1.3 — Dữ liệu kho cơ bản và doanh thu mẫu

Em là Công Thành, có công cụ hỗ trợ, tiếp nhận kết quả và lập bàn giao ngày **11/11/2026**. Ngày 12/11 trong lịch là kế hoạch, không phải ngày actual. Jira **SCRUM-127** đã map 5.1.3, Story SCRUM-107. Người nhận: **Thuận/Khang** nhận dữ liệu và luồng demo; **Phước Thuần** nhận đầu vào cho **5.5.2** sau review/tích hợp.

## Em tiếp nhận và bổ sung gì

Em fetch main **dbfa03d6011608f81b07e806c071133a393b0fce** và tạo nhánh riêng `docs/SCRUM-127-wbs-5.1.3-stock-revenue`. Main này đã có WBS 5.1.1 (b7184c7, PR39) và đầu vào 5.1.2 (6f99364, PR46).

Seed `BF-WBS5-v1` trong `database/005-wbs-demo-data.sql` đã chuẩn bị cả kho và doanh thu dùng chung 5.1.1–5.1.3. Em dùng lại wrapper, SQL fixture, verifier và kết quả đã chạy; không viết lại seed hoặc nhận phần core thành công việc mới. So sánh từ candidate đã kiểm bf69ad0 tới main hiện tại: các script seed/verifier, repository kho/doanh thu, ManagementStock và ManagementApiClient không đổi; MvpService chỉ thêm guard khóa Admin đang phân công trong SaveUser, không đổi luồng dữ liệu demo này.

Đầu ra mới là bảng fixture/expected, luồng demo, checklist và chỉ mục minh chứng WBS-5.1.3; kèm một ảnh kho cũ đúng phạm vi. **Không thêm dữ liệu, không chạy lại seed/build/unit/API/SQL/browser, không đổi mật khẩu hoặc ghi DB trong task này.** Chỉ đối chiếu source/file đã lưu và ảnh báo cáo cũ. Kết quả runtime dưới đây là lượt thật ngày 10/11, không gán thành lượt chạy mới ngày 11/11.

## Fixture kho và số liệu kỳ vọng

Nguyên liệu riêng: **WBS5 - Cốt trà đào**, đơn vị **lít**, ngưỡng **8** tại cả ba chi nhánh. Script tạo sáu phiếu trong transaction cùng tồn cuối, có audit reason `BF-WBS5-v1 demo inbound/outbound`.

| Chi nhánh | Phiếu IN: trước + thay đổi → sau | Phiếu OUT: trước + thay đổi → sau | Tồn cuối | Cảnh báo |
|---|---|---|---:|---|
| BF-DEMO-01 | 0 + 10 → 10 | 10 − 3 → 7 | 7 lít | Dưới ngưỡng 8 |
| BF-DEMO-02 | 0 + 20 → 20 | 20 − 5 → 15 | 15 lít | Đủ tồn |
| BF-DEMO-03 | 0 + 5 → 5 | 5 − 1 → 4 | 4 lít | Dưới ngưỡng 8 |

Nhận diện theo tên nguyên liệu và audit, không lấy mọi phiếu IN/OUT của DB làm sáu phiếu fixture. Ảnh CN01 có các phiếu/nguyên liệu FE khác; chỉ hàng WBS5 và cặp phiếu fixture liên quan được dùng đối chiếu.

## Fixture doanh thu mẫu

| Case / mã Test ngày 10/11 | Chi nhánh | Trạng thái | Tổng đơn | Đóng góp doanh thu |
|---|---|---|---:|---:|
| completed-a / BF-DEMO-01-20261110-0006 (ID20) | 01 | COMPLETED | 98.000đ | 98.000đ |
| cancelled-a / ...0007 (ID21) | 01 | CANCELLED | 147.000đ | 0đ |
| review-a / ...0008 (ID22) | 01 | NEEDS_REVIEW | 49.000đ | 0đ |
| completed-b / BF-DEMO-02-20261110-0001 (ID23) | 02 | COMPLETED | 30.000đ | 30.000đ |

Doanh thu **riêng fixture**: CN01 **98.000đ**, CN02 **30.000đ**, CN03 **0đ**, tổng **128.000đ**. Đơn mẫu có size/topping theo 5.1.2; snapshot tĩnh từ SQL, không nhận đã chạy workflow sửa/hủy để tạo trạng thái.

Màn doanh thu là tổng cả dữ liệu thuộc bộ lọc, có thể lớn hơn 128.000đ. Báo cáo UI gốc ghi 128.020,50đ do có 20,50đ cũ; báo cáo retest sau đó ghi 215.020,50đ do thêm đơn khác. Không dùng các số lịch sử đó làm tổng DB hiện tại, không xóa đơn cũ để ép tổng bằng fixture. UI core định dạng N0 có làm tròn phần lẻ; API/SQL giữ decimal, nên ảnh tổng nguyên đồng phải đối chiếu cùng quy tắc hiển thị và thời điểm.

## Thao tác tạo / chạy lại đã có

DB local phải có baseline/reference/DemoSeed của core và 3 chi nhánh, 9 tài khoản bao phủ 4 vai trò. Không chạy baseline lại trên DB đang có.

```powershell
# Chỉ là hướng dẫn tái lập, không phải lệnh mới đã chạy trong WBS 5.1.3:
./tools/Seed-WbsDemo.ps1 -Database BranchFlow_Test
./tools/Seed-WbsDemo.ps1 -Database BranchFlow_Test
# API trỏ đúng Test; BRANCHFLOW_DEMO_PASSWORD được cấp riêng qua process env:
./tests/sprint5/Verify-WbsDemoData.ps1 -Database BranchFlow_Test -BaseUrl https://localhost:17217
```

Wrapper gọi 003 và 005, giữ tài khoản/mật khẩu/trạng thái, so fingerprint trong bộ nhớ; seed có marker, không nhân đôi và dừng khi fixture đã đổi thay vì ghi đè. Giữ menu/chi nhánh/tài khoản đã có; bộ 62 món là tùy chọn, lượt gốc không áp dụng 004. Test/Dev có ID khác nhau; tra `AuditLog.EntityName=BF-WBS5-v1` và JSON Case, không hard-code ID Test vào ứng dụng. Đây là seed local, không tự chạy trên public/production.

## Expected / actual và nguồn minh chứng

| Case | Expected | Actual đã lưu | Trạng thái |
|---|---|---|---|
| Seed Test/Dev và chạy lại | Không đổi tài khoản, không tạo trùng | Log cũ: fingerprint khớp; 4 đơn, 6 phiếu, ID/mã giữ nguyên | Pass dùng lại 10/11 |
| Kho SQL | Tồn 7/15/4, ngưỡng 8, đủ 6 phiếu fixture | Log seed/rerun khớp; SQL 005 có guard đếm phiếu/đối chiếu tồn | Pass dùng lại 10/11 |
| Kho API | Tồn khớp SQL từng chi nhánh | Cả DemoApiResult và WBS-5.1.2/API-SQL ghi Pass BF-DEMO-01=7, 02=15, 03=4 | Pass dùng lại 10/11 |
| Kho UI CN01 | WBS5=7 lít, ngưỡng 8, dưới ngưỡng, Owner chỉ xem | Ảnh Owner cũ và case U02 trong báo cáo UAT khớp | Pass lịch sử 10/11; xem giới hạn provenance dưới đây |
| Kho UI CN02/CN03 riêng | Tồn 15/4 khi mở từng chi nhánh | Chưa có ảnh/lượt riêng được xác nhận trong bộ nguồn đã chọn | NotRun riêng; API/SQL có kết quả |
| Doanh thu fixture | COMPLETED=128.000đ; loại hủy/cần xem lại | Guard SQL + SQL manifest và snapshot bốn đơn khớp | Pass dùng lại 10/11 |
| Doanh thu ngày cả DB | API bằng SQL, tính cả đơn cũ đúng bộ lọc | Verifier đã ghi Pass; báo cáo UI lịch sử tách số liệu cùng thời điểm | Pass dùng lại 10/11 |
| Tổng duyệt local | Có người/ngày/build và điểm chưa đạt | Phiếu 5.1.2, log và ảnh cũ có lượt Công Thành thao tác; không phải QA độc lập | Dùng lại kết quả gốc |
| Source/minh chứng tiếp nhận | Script và nguồn đã kiểm giữ nguyên | Kiểm file/Git blob, lập JSON nguồn; ảnh kho copy nguyên byte và SHA256 khớp | Pass kiểm tra tài liệu 11/11 |
| Runtime mới riêng task 5.1.3 | Chỉ cần khi thay đổi/phát hiện thiếu ảnh hưởng | Không sửa ứng dụng/dữ liệu, không chạy lại | NotRun mới, dùng lại Pass gốc |
| Khang review / Thuận QA / public | Người được giao và URL/build thật | Chờ review, QA/tổng duyệt nhóm; public chưa có trong bộ này | NotRun |

Log WBS 5.1.2 có candidate **bf69ad0**, .NET 10 Release, Test, API 17217/Web 17232; 23 assertion API/SQL đạt. Đây là kết quả 10/11, không nhận đã test main dbfa03d mới.

Ảnh kho dùng lại từ báo cáo local `Sprint5-UAT-Responsive.md`, Chrome/Test, Web 17132 → API 17107, ngày 10/11. Báo cáo gốc ghi nền 47541ee + working tree, chưa pin được SHA build chính xác của ảnh kho. Em giữ rõ hạn chế đó trong `WBS-5.1.3-Historical-UI-Context.md`, không gán ảnh thành kiểm UI main hiện tại. Không dùng ảnh dựng hoặc ảnh phiếu FE làm chứng cứ tạo phiếu WBS5.

## Luồng demo bàn giao Thuận/Khang

1. Người chạy dùng build đã tích hợp, cấu hình đúng DB Test và thông tin demo cấp riêng; đọc manifest trước, không reset dữ liệu.
2. Owner → Kho → chọn CN01 → hàng WBS5: 7 lít, ngưỡng 8, dưới ngưỡng; mở IN/OUT tương ứng để đối chiếu 0→10→7. Chỉ xem, không ghi phiếu làm thay đổi fixture dùng chung.
3. QA mở tiếp CN02/CN03 theo expected 15/4 và 0→20→15 / 0→5→4; đây là ca cần người nhận ghi actual nếu muốn khép kín ảnh UI cả ba chi nhánh.
4. Owner/Admin → Tổng quan → lọc **ngày fixture thật từ BusinessDate/CreatedAt VN**, ở Test gốc là 10/11/2026; so tổng API/SQL toàn khoảng và tách phần đóng góp fixture 128.000đ. Không chọn ngày hiện tại rồi cho rằng đơn cũ bị mất.
5. Admin CN01 đối chiếu phạm vi 01; Employee xem đơn của mình theo đầu vào 5.1.2. Không nhận luồng xem dữ liệu là kiểm concurrency/rollback hoặc nghiệm thu nghiệp vụ kho nâng cao.

Em bàn giao Khang tài liệu/commit để review; Thuận nhận fixture, bảng before/change/after, expected và log/ảnh cho QA/tổng duyệt. Phước Thuần nhận đầu vào 5.5.2 sau review/tích hợp; không nhận đã làm thay WBS đó. ZIP Jira giữ đường dẫn nguồn để mở được; JSON ghi rõ nguồn commit và ngày chạy gốc. Không tự Done cả SCRUM-127/Story SCRUM-107 hoặc ký thay người nhận; không copy estimate 2h làm actual.
