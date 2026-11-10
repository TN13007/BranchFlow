# WBS-4.4.1 — Kịch bản UAT bốn vai trò

Em là Công Thành, soạn/đối chiếu ngày **10/11/2026**. Đây là bản kịch bản để chạy và chuyển QA; kết quả thực tế được tách ở [02-Ket-qua-local.md](02-Ket-qua-local.md), không dùng cột expected làm kết quả nghiệm thu.

Phạm vi: đăng nhập, menu/giỏ/tạo đơn, tra cứu, sửa số ly/hủy có lý do, danh mục, hồ sơ trong quyền hiện có, kho nhập/xuất thủ công, doanh thu và cách ly chi nhánh. Không nghiệm thu Payroll/Transfer/Stocktake hoặc workflow báo sai/duyệt nhiều bước.

## Điều kiện chạy

- Nhận Web/API, candidate SHA và dữ liệu demo từ nhóm; ghi thêm working-tree changes nếu build chưa phải commit sạch. Không gọi SHA nền là SHA chứa mọi thay đổi.
- Local: Web17132/API17107, BranchFlow_Test. Tài khoản Owner, Admin Tổng, Admin CN01, Employee A; dùng Employee B/CN02 để thử cách ly. Không ghi mật khẩu/token vào phiếu.
- Dữ liệu có menu size/topping, đơn theo trạng thái và tồn kho. Lấy baseline doanh thu/tồn trước khi chạy, không dùng tổng cứng nếu DB có giao dịch khác. Dùng đơn mới của lượt chạy để sửa/hủy, giữ fixture cũ.
- Người chạy ghi tên/ngày/candidate/actual/lỗi/minh chứng; chỉ Pass khi đã thao tác và đối chiếu. Mỗi vai trò là tài khoản kiểm tra, không coi là bốn người tham gia.

## Các ca và điều kiện chấp nhận

| Mã | Vai trò | Bước chính | Expected / điều kiện chấp nhận |
|---|---|---|---|
| U01 | Owner | Đăng nhập, mở doanh thu ngày/các chi nhánh | Vào đúng vai trò; tổng và phạm vi khớp API/SQL cùng thời điểm |
| U02 | Owner | Xem kho, cảnh báo và lịch sử | Tồn/ngưỡng/IN-OUT đúng; không có form ghi |
| U03 | Owner | Mở đơn đã hủy | Đúng snapshot; không sửa/hủy |
| U04 | Admin Tổng | Đăng nhập, xem doanh thu hệ thống | Xem được các chi nhánh trong quyền, tổng khớp Owner cùng thời điểm |
| U05 | Admin Tổng | Tìm và mở sửa món có nhiều size | Đúng món/size/giá; chưa tính mở form là đã kiểm tất cả CRUD |
| U06 | Admin Tổng | Mở hồ sơ Employee A | Đúng hồ sơ/form trong quyền; không tự thay tài khoản nếu chỉ xem |
| U07 | Admin CN01 | Đăng nhập, xem doanh thu | Chỉ CN01; tổng khớp SQL của CN01 |
| U08 | Admin CN01 | Mở/ngừng bán món tại CN01, trả trạng thái cũ | Thay đổi khả dụng chi nhánh; không sửa giá chung |
| U09 | Admin CN01 | Nhập2 nguyên liệu test | Phiếu được tạo, tồn tăng đúng2, có before/after |
| U10 | Admin CN01 | Xuất vượt tồn | Lỗi rõ; tồn và số phiếu không đổi |
| U11 | Admin CN01 | Xuất hợp lệ2 | Tồn giảm đúng2, lịch sử khớp và không thêm phiếu lặp |
| U12 | Employee A | Đăng nhập, mở menu CN01 | Đúng món đang bán/phạm vi; món bị tắt không được bán |
| U13 | Employee A | Tìm từ khóa không có, bỏ lọc | Có trạng thái rỗng; bỏ lọc khôi phục món |
| U14 | Employee A | Thêm món chưa chọn size | Báo cần size, không thêm sai vào giỏ |
| U15 | Employee A | M/3Trân châu/1Thạch,2ly | Giới hạn topping đúng; đơn giá49.000, tổng98.000 |
| U16 | Employee A | Xác nhận đơn một lần | Một mã đơn, snapshot và tổng máy chủ khớp giỏ |
| U17 | Employee A | Tìm mã không tồn tại rồi mã vừa tạo | Empty rõ; tìm đúng mã ra đúng đơn |
| U18 | Employee A | Mở trực tiếp đơn Employee B/CN02 | Không lộ chi tiết ngoài phạm vi |
| U19 | Admin CN01 | Sửa2→3ly, để trống lý do | Có lỗi; tổng lưu/lịch sử chưa đổi |
| U20 | Admin CN01 | Lưu3ly với lý do | Tổng147.000 từ snapshot49.000/ly; có lịch sử trước/sau |
| U21 | Admin CN01 | Xác nhận hủy với lý do | CANCELLED; giữ đơn/lịch sử; không còn form ghi; không tính doanh thu |
| U22 | Employee A | Xem lại cùng mã sau hủy | Thấy hủy và snapshot147.000; không có quyền sửa |
| U23 | Admin CN01 | Ngắt đúng API Test khi đọc doanh thu | Loading/lỗi/retry rõ, không hiện tổng0 giả |
| U24 | Admin CN01 | Khôi phục API Test, Thử lại | Dữ liệu phục hồi, lỗi hết; không cần tạo lại dữ liệu |
| U25 | Employee A | Nhấn xác nhận hai lần liên tiếp | Trong lượt chạy chỉ tạo một đơn; không suy thành API có idempotency |
| U26 | Owner | Đọc hồ sơ nhân viên chỉ xem | Chỉ chạy khi PM/BE chốt quyền và contract; nếu chưa có thì Blocked |
| U27 | Employee A | POST đã lưu nhưng nhận401; đăng nhập lại | Giỏ/cảnh báo còn; không tự gửi lại; kiểm lịch sử trước quyết định tiếp |
| U28 | Employee A | POST đã lưu nhưng client timeout | Giữ giỏ/cảnh báo, khóa gửi lại; lịch sử có đúng đơn đã lưu |

Chạy U01–U18 trước, sau đó U19–U22 trên cùng mã đơn. U23/U24 chỉ ngắt host Test; U25–U28 dùng ca/host riêng, ghi rõ lỗi được giả lập và thao tác thực sự gửi. Không phá host Dev hoặc dữ liệu của người khác.

## Smoke public sau deploy — chưa chạy

| Mã | Luồng | Đầu vào bắt buộc | Expected / chấp nhận | Trạng thái hiện tại |
|---|---|---|---|---|
| P01 | HTTPS và đăng nhập bốn role | URL Web/API, deployedSHA, tài khoản được cấp | Chứng chỉ hợp lệ; đúng vai trò/phiên | NotRun |
| P02 | Gọi món và tra cứu | Dữ liệu demo/nhánh thử được phép ghi | Một đơn, snapshot và tìm kiếm đúng | NotRun |
| P03 | Kho/doanh thu | Quyền/fixture được PM cho phép | Đúng dữ liệu và phạm vi; không phá dữ liệu thật | NotRun |
| P04 | Ngoài chi nhánh | Hai tài khoản khác branch | Từ chối và không lộ dữ liệu | NotRun |
| P05 | Đăng xuất/mất phiên | Candidate public và phiên thử | Không giữ quyền sai; không tự gửi lại POST chưa rõ kết quả | NotRun |

Khang bàn giao URL/SHA; Thuận chạy public/ghi actual, Thành hỗ trợ theo phân công. Chưa có URL/SHA thì để trống người chạy/ngày thực tế và giữ NotRun; không ghi localhost là public.
