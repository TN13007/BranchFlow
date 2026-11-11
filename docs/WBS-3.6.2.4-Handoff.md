# WBS-3.6.2.4 — Xử lý trạng thái trống/lỗi và số liệu

Em là Công Thành, có công cụ hỗ trợ, tiếp nhận và lập bàn giao ngày **11/11/2026**. Jira **SCRUM-126** đã map cả 3.6.2.3 và 3.6.2.4, Story SCRUM-105. Người nhận tiếp: **Hòa Thuận — WBS 3.6.2.5**; Khang review cuối, QA ghi kết quả retest thực tế.

## Đầu vào và phần em làm

Em tiếp nhận commit **1488e9e5143147dc5267dd4607a5cb0e6881e38a** của WBS 3.6.2.3. Phần loading/empty/error và dữ liệu doanh thu đã có code và lượt kiểm thật, nên em dùng lại kết quả đúng phiên bản, không viết lại code hoặc chạy lại các ca không bị ảnh hưởng để tính thêm công.

Em fetch main **dbfa03d6011608f81b07e806c071133a393b0fce**, tạo nhánh riêng `docs/SCRUM-126-wbs-3.6.2.4-revenue-states`, rồi fast-forward nhận commit đầu vào 1488e9e. Tại lúc bàn giao, đầu vào **chưa nằm trong origin/main**. Nhánh này có đủ đầu vào để review; cần Khang tích hợp/review predecessor, không lấy main chưa tích hợp rồi cho rằng có sẵn bản sửa.

Đầu ra mới của em gồm tài liệu này và `WBS-3.6.2.4-Evidence.json`: checklist AC, expected/actual, nguồn minh chứng và Git blob đối chiếu. **Không sửa ứng dụng, không tạo thêm fixture, không đổi tài khoản/DB, không chạy lại build/unit/HTTP/browser trong WBS 3.6.2.4.** Kiểm tra mới là đối chiếu source và artifact đã lưu với commit đầu vào; không gọi việc đó là một lượt nghiệm thu runtime mới.

## Hành vi đã có và kết quả dùng lại

Môi trường của lượt kiểm gốc: Windows, .NET 10 Release, Chrome; SQL localhost/BranchFlow_Test; API https://localhost:17317; Web https://localhost:17332. Đây là host local tạm, không phải public. Ảnh UI gốc ngày **10/11/2026** dùng main ae8c7f2 + component blob **f51232d3781586af6884e495ce02b846d553018e**. Build/unit/HTTP retest gốc ngày **11/11/2026** dùng main dbfa03d + cùng component. Có hai file provenance phân biệt rõ.

| AC / case | Expected | Actual đã ghi nhận | Kết quả / nguồn gốc |
|---|---|---|---|
| Loading ban đầu và khi đổi lọc | Hiện Đang xử lý; khóa lọc khi request đang chạy; chưa hiện tổng/bảng | Đạt; ảnh branchadmin-loading.png với read SQL Test trì hoãn rồi rollback | Pass, dùng lại UI 10/11 |
| Ngày không có dữ liệu | API []; UI tổng 0 và thông báo không có đơn hoàn thành | Đạt với 01/01/2001; owner-empty.png | Pass, dùng lại UI 10/11 và HTTP 11/11 |
| Lỗi khoảng ngày | API 400, UI báo rõ; không diễn giải lỗi thành tổng 0 hoặc trống | Đạt với from 02/11 > to 01/11; owner-invalid-range.png | Pass, dùng lại UI 10/11 và HTTP 11/11 |
| Phục hồi sau lỗi | Chọn lại khoảng đúng, hết lỗi và trả đúng dữ liệu | CN02 trở lại 300.000đ | Pass, dùng lại ghi nhận trong handoff 3.6.2.3 |
| Biên nửa đêm VN | 23:59:59 thuộc 01/11; 00:00:00 thuộc 02/11 | CN01 ngày 01 = 100.000đ; ngày 02 = 200.000đ | Pass, dùng lại UI/API/SQL |
| Tổng và đơn hủy | 3 đơn hoàn thành = 600.000đ, loại CANCELLED 999.000đ | UI/API/SQL khớp từng dòng; owner-all-600000.png | Pass, dùng lại UI/API/SQL |
| Lọc chi nhánh | Owner chọn CN02 = 300.000đ; Admin CN01 chỉ CN01 = 300.000đ | Khớp; dropdown Admin không có CN02; branchadmin-300000.png | Pass, dùng lại UI/API/SQL |
| Không lộ dữ liệu qua gọi trực tiếp | Admin CN01 query CN02 = 403; Employee = 403; chưa đăng nhập = 401 | Đúng HTTP status, không trả doanh thu ngoài quyền | Pass, dùng lại HTTP 11/11 |
| Owner chỉ đọc | Không có nút CRUD doanh thu; PUT chi nhánh bị chặn | UI chỉ lọc/xem; HTTP PUT trả 403 | Pass, dùng lại UI/HTTP |
| Build/unit | Build API/Web đạt; validation ngày đúng | 0 warning/error; 5 unit Pass, 0 Fail/Skip; verifier HTTP/SQL 25 Pass | Pass, dùng lại log 11/11 |
| Source/artifact của nhánh nhận | Trùng byte Git với đầu vào 1488e9e | Đã đối chiếu blob source và các file minh chứng, ghi trong JSON | Pass, kiểm tra tài liệu/source ngày 11/11 |
| Runtime mới riêng WBS 3.6.2.4 | Chỉ chạy lại nếu code/build liên quan thay đổi | Không thay đổi source so với đầu vào; không chạy lại | NotRun mới; dùng lại các Pass gốc phía trên |
| Khang review / Thuận QA retest | Người được giao thực sự kiểm và xác nhận | Chờ bàn giao | NotRun |
| Smoke public | Có URL/build triển khai thật | Chưa có trong phạm vi lượt kiểm này | NotRun |

Fixture gốc là bốn header SQL tĩnh ID 35–38, mã WBS3623-BEFORE/MIDNIGHT/CANCELLED/BRANCH-B; không phải bằng chứng đã chạy workflow tạo/chuyển trạng thái đơn. Không seed bộ mới. Bảng hiển thị dd/MM/yyyy và VND; input date native theo locale Chrome/Windows (ảnh ô lọc MM/DD/YYYY), giá trị API ISO yyyy-MM-dd. Không nhận đã đổi locale hệ điều hành hay nghiệm thu màn hình mới.

## Minh chứng và cách QA nhận

- Code và kết quả gốc: [commit 1488e9e](https://github.com/TN13007/BranchFlow/commit/1488e9e5143147dc5267dd4607a5cb0e6881e38a).
- Handoff gốc: `docs/WBS-3.6.2.3-Handoff.md`; [comment Jira 10277](https://khangba153.atlassian.net/browse/SCRUM-126?focusedCommentId=10277).
- 7 ảnh thật giữ nguyên tên/đường dẫn `docs/evidence/WBS-3.6.2.3/`; không chuyển thành ảnh mới của ngày 11/11.
- Script/log: `tests/sprint5/WBS-3.6.2.3/`, ưu tiên HTTP-SQL-2026-11-11.txt, Unit-Revenue-2026-11-11.txt, Build-Api-2026-11-11.txt, Build-Web-2026-11-11.txt, SQL-Totals.txt và hai provenance.
- JSON chỉ mục tại `docs/WBS-3.6.2.4-Evidence.json` pin source commit/blob. ZIP bàn giao có hai tài liệu mới cùng bộ gốc giữ nguyên thư mục để QA mở được khi tải từ Jira; đây là đóng gói dùng lại, không nhận tác giả/lượt chạy mới.

Thuận nhận checklist/fixture/script để thực hiện **3.6.2.5**, ghi SHA và actual của chính lượt QA; em không ký thay hoặc nhận đã xong WBS của Thuận. Khi QA chạy lại cần host Test và mật khẩu hiện có cấp ngoài Git; hướng dẫn lệnh ở handoff 3.6.2.3. Nếu main/build sau tích hợp đổi luồng doanh thu, QA kiểm lại phần bị ảnh hưởng; nếu không, có thể đối chiếu kết quả gốc thay vì chạy lặp toàn bộ.

Chờ Khang review/tích hợp đầu vào, QA và public. Không tự đóng toàn bộ SCRUM-126 hoặc Story SCRUM-105, không copy estimate 2h làm giờ thực tế.
