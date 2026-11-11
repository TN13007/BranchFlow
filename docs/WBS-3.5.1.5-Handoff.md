# WBS-3.5.1.5 — Xem tồn kho / phiếu giao dịch

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, bổ sung kiểm chứng và bàn giao **11/11/2026**. Ngày 13/11 trong lịch là kế hoạch. Jira đúng WBS: **SCRUM-125**, Story SCRUM-104. Nhánh riêng: `feat/SCRUM-125-wbs-3.5.1.5-stock-ui` từ main **af77b9e**.

## Phần dùng lại và phần em đưa vào nhánh

Core đã có schema/model Ingredient, BranchIngredient, StockTransaction, API nhập/xuất và màn kho. Em giữ nguyên backend/schema, không tạo trùng. Nhận đầu vào API 3.5.1.4 của Trường Tài; các ca quyền và ghi tồn vẫn do server kiểm.

Em tìm lại phần sửa **ManagementStock.razor** đã làm ở working tree cũ, đúng phạm vi task nhưng chưa vào main, và đưa nguyên file đó sang nhánh này; không nhận đây là viết mới lại core. Chỉ file UI kho được nhận, không kéo CSS/menu/workspace/test khác đang sửa ở thư mục cũ.

- Tải lại tồn và lịch sử, có retry khi API lỗi; khóa thao tác lúc đang gửi/chưa tải đủ/mất phiên/ngoài quyền.
- Trạng thái loading và tồn/lịch sử trống rõ; số lượng/đơn vị và dấu thay đổi IN/OUT hiển thị theo vi-VN.
- Khi POST kho gặp lỗi mạng/timeout/401/5xx, báo chưa xác định kết quả và khóa ghi tiếp tới khi tải lại/kiểm tra lịch sử. Không tự gửi lại POST. Đây không phải bảo đảm idempotency hoặc lưu cảnh báo qua restart/relogin.
- Giữ Owner chỉ xem; Admin có nhập/xuất thủ công, số lượng dương, lý do và RowVersion theo contract. HTTP 409 hiện lỗi; người dùng tải lại để lấy tồn/phiên bản mới.

Em dùng lại báo cáo HTTP/UI ngày 10/11. Chỉ bổ sung **một test SQL tích hợp kho** vì log cũ tổng hợp chưa ghi riêng rõ ca đồng thời; test có tên đầy đủ trong log mới. Sau nhận file UI em build Web Release; không chạy lại toàn suite hoặc runner 62 check.

## Môi trường / phiên bản

Lượt mới: Windows, .NET 10 Release; SQL **localhost/BranchFlow_Test**, main af77b9e; `StockTransactionIntegrationTests.WriteStockAsync_ValidatesStockQuantityRowVersionAndConcurrency`. Test gọi service/repository với hai DbContext riêng, ghi SQL thật; không gọi HTTP hoặc đăng nhập mật khẩu. Dùng nguyên liệu GUID riêng, dọn dữ liệu của chính test trong finally. Không thay tài khoản/mật khẩu hoặc ghi Dev.

UI/HTTP cũ: báo cáo ghi nền **47541ee + working tree**, chưa pin được SHA runtime chính xác; Web17132 → API17107 → BranchFlow_Test, Chrome Windows, ngày **10/11/2026**. Giữ nguyên nguồn/ngày trong tài liệu trích, không gọi ảnh cũ là ảnh main hiện tại. Component đưa vào nhánh trùng byte file kho local được tiếp nhận; Git blob và build mới ghi tại `Provenance.txt`.

## Case / expected / actual

| Ca | Expected | Actual / nguồn | Kết quả |
|---|---|---|---|
| IN10 | Tồn 0→10, phiếu +10 | Test SQL mới kiểm tồn, RowVersion và history | Pass 11/11 |
| OUT3 | Tồn 10→7, phiếu −3 | Test SQL mới; HTTP cũ cũng ghi 10−3=7 | Pass mới + dùng lại |
| OUT8 khi tồn7 | Bị chặn, tồn7, không có phiếu mới | Test SQL mới so count trước/sau; HTTP cũ trả400 | Pass mới + HTTP dùng lại |
| RowVersion cũ | Không ghi đè, không thay tồn bởi request bị từ chối | SQL mới: OUT1 hợp lệ 7→6, replay version cũ ném concurrency; HTTP cũ xác nhận409 | Pass mới + HTTP dùng lại |
| Hai lệnh OUT4 cùng version khi tồn6 | Chỉ một thành công, tồn2, thêm đúng1phiếu, không âm | Test SQL mới Task.WhenAll, hai context; assertions history/audit đạt | Pass 11/11, service/SQL; không nhận là hai request HTTP mới |
| Cross-branch | Admin CN01 không ghi/đọc CN02 | Test SQL mới từ chối ghi; runner HTTP cũ đã kiểm GET stock ngoài branch403 | Pass mới + HTTP dùng lại |
| Lịch sử dấu/đơn vị | IN(+), OUT(−), before/change/after khớp | SQL mới kiểm bốn chi tiết (0,+10,10), (10,−3,7), (7,−1,6), (6,−4,2) và audit | Pass 11/11 |
| Tạo đơn không tự trừ kho | Tồn trước/sau tạo đơn bằng nhau | HTTP cũ và UAT SQL cũ ghi Pass; test mới không tạo đơn | Pass dùng lại 10/11 |
| UI xem tồn/phiếu | Owner chỉ đọc, đúng tồn/đơn vị/cảnh báo; Admin thấy IN/OUT và before/after | Ảnh Owner WBS5=7lít/ngưỡng8; ảnh Admin phiếu FE #19:9→7(-2), báo cáo U02/U09–U11 | Pass lịch sử 10/11; không gán ảnh FE thành fixture SQL mới |
| Chặn thao tác lặp / loading / empty / error | busy guard và fieldset khóa, có trạng thái/refresh | Có trong code nhận và báo cáo cũ; chưa có lượt browser mới riêng trên main af77b9e | Đối chiếu source; dùng lại phạm vi UI cũ, không nhận mọi biến thể đã runtime Pass |
| Build file UI đã nhận | Web Release thành công | 0 warning, 0 error | Pass 11/11 |
| Test mục tiêu | Có tên ca, không skip | 1 Pass, 0 Fail, không skip | Pass 11/11 |
| Cleanup / fixture dùng chung | Không giữ nguyên liệu test; không thay tồn BF-WBS5 | SQL sau test: 0 nguyên liệu __TEST_STOCK_; WBS5 giữ7/15/4, ngưỡng8 | Pass 11/11 |
| Fault-inject timeout/401/5xx POST kho qua browser | Xác nhận cảnh báo/khóa và đối soát thực tế | Báo cáo cũ ghi chưa fault-inject; không lấy ca POST đơn hàng thay thế | NotRun; QA bổ sung |
| Khang review, Thuận QA / public | Người được giao kiểm, có URL/build thật | Chờ review/QA; public chưa chạy | NotRun |

Ca đồng thời ghi ở cấp service/SQL; việc mapper trả409 qua HTTP được dùng lại từ ca replay cũ, không tự gán status HTTP cho test SQL. Log 62 checks là tổng runner nhiều nghiệp vụ; trích vài dòng kho không có nghĩa đã chạy 62 case kho.

## Minh chứng / tái lập

- `tests/sprint5/WBS-3.5.1.5/Stock-Integration-2026-11-11.txt`: lượt SQL mục tiêu mới, tên test và kết quả.
- `Fixture-After-Focused-Test.txt`, `Build-Web-2026-11-11.txt`, `Provenance.txt`: cleanup/fixture, build và source.
- `Historical-HTTP-Excerpt.txt`, `docs/WBS-3.5.1.5-Historical-UI.md`, hai ảnh ở `docs/evidence/WBS-3.5.1.5/`: nguồn cũ, giữ ngày10/11. Ảnh không sửa, SHA256 nguồn/bản kèm khớp.
- `WBS-3.5.1.5-Evidence.json`: đường dẫn/commit/blob/SHA256 và giới hạn dùng lại. Không kèm báo cáo toàn bộ WBS khác hoặc secret.

Lệnh mục tiêu đã chạy trên Test:

```powershell
# Cấp BRANCHFLOW_TEST_CONNECTION_STRING qua process env trỏ local BranchFlow_Test, không ghi secret vào Git.
dotnet test tests/BranchFlow.Tests -c Release --filter FullyQualifiedName~StockTransactionIntegrationTests --logger "console;verbosity=normal"
dotnet build src/BranchFlow.Web -c Release
```

Không chạy baseline, reset DB hoặc thao tác trực tiếp lên seed kho dùng chung. UI smoke/retest của QA nên dùng nguyên liệu riêng cho ca ghi; không xuất quá tồn/đồng thời trên WBS5 chỉ để lấy ảnh. Không thêm kiểm kê/duyệt phiếu hoặc làm thay backend WBS của Tài.

## Bàn giao

Khang nhận commit/file UI và bằng chứng để review. Hòa Thuận nhận checklist, test mục tiêu và before/change/after cho **3.5.1.6**, cùng đầu vào **3.1.2.5**. Phước Thuần nhận build đã review/tích hợp cho **5.5.1**. Trường Tài nhận kết quả đối chiếu API đầu vào nếu cần. Đây là nội dung handoff/Jira, không giả người nhận đã xác nhận.

Phần chưa khép kín: fault-inject POST kho, browser smoke mới đúng build tích hợp và QA độc lập; không tự Done toàn bộ SCRUM-125/Story SCRUM-104 hoặc ghi estimate2h thành actual. Chỉ ghi Pass theo phạm vi và nguồn nêu trên.
