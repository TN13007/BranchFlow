# WBS-3.4.2.1 — Hiển thị yêu cầu điều chỉnh theo chi nhánh

> **Cập nhật sau bàn giao:** các ca quyền, sửa sau hủy, stale409 và rollback khi lỗi ghi chi tiết/lịch sử đã có lượt bổ sung đạt52checks. Xem [kết quả mới](WBS-3.4.2.1-Completion-2026-11-10.md); trạng thái NotRun tương ứng bên dưới là lịch sử trước đợt bổ sung, không còn là trạng thái mới nhất.

Người thực hiện: Công Thành, có công cụ hỗ trợ. Ngày thực hiện/kiểm chứng: 10/11/2026. Jira: SCRUM-124, phần Lịch / effort / WBS map 3.4.2.1 và 3.4.3.1; Story SCRUM-103. Bản này chỉ bàn giao 3.4.2.1, không chốt toàn bộ SCRUM-124.

## Branch, build và nguồn minh chứng

- Nhánh riêng: feat/SCRUM-124-wbs-3.4.2.1-handoff, tạo trực tiếp từ 47541ee trong worktree riêng. Không kế thừa commit WBS-5.1.1 và không mang thay đổi FE/CSS/test clock đang có ở workspace cũ sang.
- Build/test mới trong worktree sạch: Web .NET 10 Release 0 warning/error; unit không Integration 76/76, 0 skipped. Log trong tests/sprint5/WBS-3.4.2.1.
- SQL chỉ đọc mới: localhost/BranchFlow_Test, đơn BF-DEMO-01-20261110-0009 (ID24). Không phát lại thao tác ghi và không thay đổi Dev.
- Ảnh UI và trích log HTTP là lượt đã chạy thật trước đó cùng ngày, trên Web Test17132/API17107, candidate 47541ee + working tree lúc kiểm tra. Không nhận là ảnh vừa chụp từ worktree sạch. Logic chi tiết/sửa/hủy/lịch sử đơn có sẵn trên 47541ee; wrapper trạng thái/retry/format có thay đổi ở working tree cũ và không thuộc commit bàn giao này.

## Phần core có sẵn / phần em làm

Core đã có ManagementWorkspace hiển thị danh sách theo phạm vi/ngày, chi tiết snapshot size/topping, nhập số ly, lý do bắt buộc, xác nhận hủy, lịch sử trước/sau; đơn CANCELLED không có form ghi. ManagementController chỉ cho Admin Tổng/Admin CN gọi PUT; MvpService kiểm phạm vi, RowVersion, số ly 1–999 và tính lại tiền từ snapshot. Em tiếp nhận và kiểm chứng phần có sẵn, không viết lại hoặc nhận là tự phát triển baseline.

Em tách báo cáo WBS, chọn đúng ảnh/log liên quan, bổ sung script SQL chỉ đọc để người nhận đối chiếu cùng mã đơn, build và chạy unit trên nhánh sạch. Không sửa source ứng dụng/backend/schema trong commit này.

## Case, expected và actual

| Ca | Expected | Actual / minh chứng | Trạng thái |
|---|---|---|---|
| Admin CN01 mở đơn có size/topping | Đúng branch, cấu hình và snapshot | Trà đào cam sả M, Trân châu×3, Thạch×1; mỗi ly 49.000đ | Pass trong lượt UI đã ghi nhận |
| Sửa 2 ly thành 3 ly | 98.000đ thành 147.000đ, không đổi giá/cấu hình | Ảnh branch-order-adjusted; SQL có BeforeData98.000/AfterData147.000 và số ly3, topping/giá đúng | Pass |
| Để trống lý do khi sửa | Báo lỗi; chưa cập nhật tổng đã lưu | Ảnh branch-adjust-reason-error hiện tổng preview147.000 nhưng đã lưu98.000; hiện yêu cầu nhập 1–500 ký tự | Pass trong lượt UI đã ghi nhận |
| Hủy có lý do | CANCELLED, giữ đơn và lịch sử | Ảnh branch-order-cancelled; SQL còn đơn147.000, hai history đúng actor/lý do/thời điểm | Pass |
| Giao diện sau hủy | Không cho sửa số ly/hủy lại | Ảnh không có form/nút ghi; Employee thấy cùng mã CANCELLED147.000 | Pass cho UI; PUT sửa sau hủy chưa phát lại trong đợt này |
| Quyền Employee/Owner ghi và Admin ngoài branch | Server từ chối | Lượt HTTP tổng hợp trước đã kiểm 403; log chỉ lưu tổng hợp, không có trace từng request. Source Controller/Scope đã đối chiếu | Kết quả lịch sử; cần QA ghi trace riêng, không nhận như ca mới trên ID24 |
| Employee xem ngoài phạm vi | Không lộ dữ liệu | Ảnh employee-cross-branch-denied hiển thị không tìm thấy dữ liệu trong phạm vi | Pass trong lượt UI đã ghi nhận; không dùng thay Admin cross-branch PUT |
| Đơn hủy không tính doanh thu | Revenue chỉ lấy COMPLETED | SQL ID24=CANCELLED; query/repository dùng completed-only, log HTTP trước xác nhận loại đơn hủy | Pass trong phạm vi đối chiếu; không dùng tổng DB cũ làm tổng hiện tại |
| Stale RowVersion khi sửa/hủy | 409 và không có thay đổi dở dang | Chưa có lượt ép stale riêng cho sửa/hủy trong gói này | NotRun |
| Ghi chi tiết/lịch sử thất bại | Header/detail/history cùng rollback | Chưa có fault injection riêng cho AdjustOrderAsync; test rollback tạo đơn không thay ca này | NotRun |

Script SQL kiểm snapshot147.000, giá món30.000 + topping15.000 +4.000 mỗi ly, đúng hai history, before/after98.000→147.000 và COMPLETED→CANCELLED, actor demo.branchadmin.01. Script không chứng minh lại thao tác HTTP bị từ chối hoặc rollback.

## Chạy lại và xem minh chứng

Tại root nhánh:

```powershell
dotnet build src/BranchFlow.Web -c Release --nologo
dotnet test tests/BranchFlow.Tests -c Release --filter 'FullyQualifiedName!~Integration' --nologo
sqlcmd -S localhost -E -C -b -f 65001 -d BranchFlow_Test -W -i tests/sprint5/WBS-3.4.2.1/Verify-RecordedOrder.sql
```

- [SQL verification](../tests/sprint5/WBS-3.4.2.1/Verify-RecordedOrder.sql), [actual SQL](../tests/sprint5/WBS-3.4.2.1/Sql-2026-11-10.txt).
- [Build](../tests/sprint5/WBS-3.4.2.1/Build-2026-11-10.txt), [unit](../tests/sprint5/WBS-3.4.2.1/Unit-2026-11-10.txt), [trích HTTP lịch sử](../tests/sprint5/WBS-3.4.2.1/Prior-OrderApi-Excerpt.txt). Trích HTTP có SHA256 nguồn; không gán tất cả các assertion của runner rộng cho WBS này.
- [Sửa số ly](evidence/WBS-3.4.2.1/branch-order-adjusted-1366.png), [thiếu lý do](evidence/WBS-3.4.2.1/branch-adjust-reason-error-1366.png), [hủy/lịch sử](evidence/WBS-3.4.2.1/branch-order-cancelled-1366.png), [Employee thấy đơn hủy](evidence/WBS-3.4.2.1/employee-cancelled-1366.png), [ngoài phạm vi](evidence/WBS-3.4.2.1/employee-cross-branch-denied-1366.png).

## Tồn đọng và người nhận

Em bàn giao Tài đầu ra cho WBS3.4.2.2: mã đơn, snapshot và hai lịch sử, script đối chiếu, các ca còn thiếu. Đề nghị Tài/QA bổ sung trace sửa sau hủy, quyền Admin ngoài branch, stale409 và rollback sau lỗi ghi chi tiết/lịch sử trên đúng candidate. Khang review, Thuận QA retest độc lập. SCRUM-124 còn WBS3.4.3.1 và hồi quy giỏ401/SCRUM-109; báo cáo này không thay các đầu ra đó. Public NotRun. Không ghi giờ actual giả, chưa đề nghị Done khi còn các ca nghiệm thu thiếu.
