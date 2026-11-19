# Hướng dẫn PCG kiểm thử nghiệm thu UAT BranchFlow

**Đợt:** Sprint 5 / SCRUM-131 · **Ngày soạn:** 17/11/2026 · **Đối tượng:** ba người dùng nội bộ PCG, người điều phối và QA.

Tài liệu này là kịch bản thao tác trên **Web demo thật**. Kết quả chỉ được ghi **Pass** sau khi chính người tham gia thực hiện và xác nhận; tài liệu chuẩn bị sẵn không phải biên bản nghiệm thu. Mỗi người tham gia phải tạo ít nhất **một đơn riêng**. QA hỗ trợ quan sát và ghi lỗi, nhưng không thao tác hay xác nhận thay họ.

## 1. Chuẩn bị trước buổi UAT

1. Người điều phối xác nhận với Khang URL Web, URL API, phiên bản đang triển khai (commit SHA hoặc mã bản build), cơ sở dữ liệu demo, thời gian kiểm thử và ba người tham gia. URL Web nhóm cung cấp gần nhất: <https://branchflow-team-web-khang153.azurewebsites.net/login>. Mở được trang đăng nhập chưa đủ để xác nhận phiên bản đang chạy. Ghi phiên bản được chủ bản phát hành xác nhận vào biên bản.
2. Dùng **máy tính hoặc điện thoại có trình duyệt hiện đại**, kết nối Internet ổn định. Mở cửa sổ riêng cho mỗi lượt nếu dùng chung máy; đăng xuất và đóng cửa sổ giữa hai người. Ghi tên trình duyệt, thiết bị và giờ Việt Nam.
3. Người điều phối cấp tài khoản demo phù hợp và mật khẩu qua kênh riêng. Ví dụ tài khoản Employee: `demo.employee.a`; Admin Chi nhánh: `demo.branchadmin.01`. Không ghi mật khẩu, cookie, token hay ảnh màn hình chứa chúng vào Jira hoặc Git. Mật khẩu demo dùng chung **không chứng minh ai đã thực hiện**; mỗi người cần xác nhận từ tài khoản Jira của chính mình hoặc một bản ghi có thể quy thuộc rõ ràng.
4. Dành các đơn mới của buổi UAT cho kiểm thử. Chỉ sửa/hủy **đơn demo được chỉ định riêng**, sau khi người tạo đã chụp bằng chứng. Không dùng đơn của khách thật hoặc chỉnh catalog/kho đang dùng chung khi chưa thống nhất với điều phối viên.
5. Mở [biên bản SCRUM-131](../tests/sprint5/QA-SCRUM-131-UAT-Handoff-2026-11-17.md) để điền **ngay trong buổi**. Các trạng thái hợp lệ: `Pass`, `Fail`, `Blocked`, `Not Run`; không tự chuyển trường hợp chưa làm thành `Pass`.

**Nếu không đăng nhập được, sai vai trò/chi nhánh, Web/API không hoạt động hoặc chưa xác nhận được bản phát hành:** ghi `Blocked`, giờ và ảnh lỗi; nhờ điều phối viên xử lý rồi mới tiếp tục. Không thử tài khoản khác để vượt quyền.

## 2. Cách ghi một ca kiểm thử

Với từng ca dưới đây, ghi: mã ca; người thực hiện; ngày giờ Việt Nam; URL/bản build; tài khoản (chỉ tên đăng nhập), vai trò và chi nhánh; thiết bị/trình duyệt; dữ liệu đã chọn; **kết quả mong đợi**; **kết quả thực tế**; Pass/Fail/Blocked; mã đơn/phiếu; ảnh hoặc liên kết minh chứng; mã lỗi Jira nếu có. Chụp màn hình sau khi kết quả hiện ra, che thông tin bí mật. Với số tiền, lấy **giá đang hiển thị ở chính bản UAT**, không dùng giá trong tài liệu cũ làm chuẩn cố định.

Một ca `Pass` khi mọi bước chính và kết quả mong đợi khớp. `Fail` khi đã thao tác được nhưng hành vi sai. `Blocked` khi không thể kiểm tra do môi trường, quyền hoặc dữ liệu. Ghi rõ bước dừng để người khác chạy lại.

## 3. Kịch bản bắt buộc: mỗi người PCG tự tạo một đơn

Thực hiện lần lượt **UAT-01 đến UAT-05 cho từng người trong ba người**. Có thể dùng cùng một tài khoản demo Employee theo lượt nếu thiếu ba tài khoản: đăng xuất/đăng nhập giữa các lượt, ghi mã đơn và giờ riêng, rồi từng người tự xác nhận từ Jira của họ. Không chạy song song trên cùng một phiên đăng nhập.

| Mã | Thao tác cụ thể | Kết quả cần thấy / minh chứng |
|---|---|---|
| UAT-01 | Vào URL đã xác nhận, nhập tài khoản được cấp, bấm **Đăng nhập**. Xem tên vai trò và chi nhánh. | Đến trang làm việc, đúng vai trò Employee và đúng chi nhánh được phân công. Chụp màn hình sau đăng nhập, không chụp mật khẩu. |
| UAT-02 | Mở **Gọi món**. Ghi tên một món đang mở bán, chọn **Chọn món**, chọn một size, có thể chọn một topping và ghi chú nhanh. Đặt **Số lượng ly = 1**. Ghi giá món, size/topping và tổng dự kiến trước khi thêm. | Món/tùy chọn hiển thị rõ. Tổng dự kiến = số ly × (giá món theo size + tổng giá topping trên mỗi ly). Nếu dùng topping nhiều phần, tính giá theo số phần cho **mỗi ly**. |
| UAT-03 | Bấm **Thêm vào đơn**, mở giỏ hàng. Kiểm tra tên món, size, topping, ghi chú, số ly và tổng. Nếu sai, dùng **Chỉnh sửa tùy chọn** rồi kiểm lại. Bấm **Xác nhận tạo đơn** **một lần**. | Mở **Chi tiết đơn** với mã đơn chính thức, trạng thái và tổng tiền khớp dữ liệu đã chọn. Ghi mã đơn; chụp màn chi tiết. Không bấm xác nhận lần hai khi đang chờ. |
| UAT-04 | Mở **Đơn của tôi**, tìm theo mã vừa ghi và ngày kinh doanh (giờ Việt Nam) nếu cần. Bấm **Tìm đơn**, rồi **Xem chi tiết**. | Mã đơn xuất hiện **một lần**, thuộc đúng người/chi nhánh, số ly và tổng tiền khớp màn xác nhận. Chụp danh sách và chi tiết nếu có thể. |
| UAT-05 | Chính người tham gia ghi một câu nhận xét sử dụng và kết luận `Pass`/`Fail`/`Blocked` trong bình luận Jira SCRUM-131 hoặc bản ghi định danh được. Ghi mã đơn, giờ, vai trò/chi nhánh và liên kết bằng chứng; không ghi mật khẩu. | Có **ba xác nhận độc lập của ba người**, mỗi người gắn với một lượt tạo đơn và một mã đơn. QA chép liên kết xác nhận vào đúng hàng của biên bản. |

**Dữ liệu dễ làm:** chọn một món đang mở bán, một size, một ly, không topping nếu muốn kiểm luồng cơ bản. Để kiểm cách tính tiền, thêm một topping rồi ghi giá hiển thị. Không cần cố chọn món hoặc giá cố định vì menu demo có thể đổi.

**Nếu bấm xác nhận mà mạng ngắt, trang treo hoặc báo phiên hết hạn:** ghi giờ và tình huống, vào **Đơn của tôi** tìm trước. Nếu đã có đơn, dùng mã đó và không gửi lại; nếu chưa thể kết luận, đánh dấu `Blocked` và nhờ QA kiểm tra. Không bấm tạo lại liên tục vì có thể sinh đơn trùng.

## 4. Kịch bản bổ sung theo vai trò (điều phối viên phân công)

Các ca này giúp PCG đánh giá phạm vi MVP; ba đơn và ba xác nhận ở mục 3 vẫn là điều kiện riêng của SCRUM-131. Dùng **cửa sổ/phiên khác** khi đổi vai trò. Chụp cả bộ lọc và kết quả để có thể đối chiếu.

| Mã / vai trò | Các bước thao tác | Kết quả mong đợi |
|---|---|---|
| UAT-06 · Employee | Trong **Gọi món**, thử chọn quá 3 phần của cùng một topping trên một ly; thử đổi số ly; mở giỏ và quay lại. | UI giới hạn tối đa 3 phần mỗi loại topping/ly; tổng tiền cập nhật theo số ly; giỏ giữ đúng lựa chọn. |
| UAT-07 · Admin Chi nhánh | Đăng nhập `demo.branchadmin.01`, mở **Đơn hàng**, chọn chi nhánh được phép, khoảng ngày có đơn UAT, bấm **Xem kết quả**, mở **Xem chi tiết** một đơn demo được chỉ định. | Danh sách và chi tiết nằm trong phạm vi chi nhánh; thấy mã, dòng món, trạng thái, số tiền và lịch sử. Không thấy quyền sửa đơn ngoài chi nhánh. |
| UAT-08 · Admin Chi nhánh | Trên **đơn demo dành riêng cho chỉnh sửa**, sửa **Số ly** từ 1 thành 2, nhập lý do rõ ràng, bấm **Lưu số ly**. Tải lại chi tiết. | Số ly/tổng mới được lưu theo giá và topping snapshot của đơn; **Lịch sử chỉnh sửa** có trước/sau, lý do, người và thời điểm. Ghi mã đơn và số tiền trước/sau. |
| UAT-09 · Admin Chi nhánh | Trên **một đơn demo khác dành riêng cho hủy**, nhập lý do, bấm **Hủy đơn**, kiểm tra mã ở hộp xác nhận rồi bấm **Xác nhận hủy đơn**. | Trạng thái thành **Đã hủy**, lịch sử có lý do; đơn không còn tính vào doanh thu COMPLETED. Không hủy một trong ba đơn bắt buộc trước khi người tham gia đã xác nhận. |
| UAT-10 · Admin Chi nhánh | Mở **Menu** và xem khả dụng món ở chi nhánh. Nếu có món demo dành riêng, thử **Ngừng bán** rồi **Mở bán** trở lại sau khi các lượt tạo đơn xong. | Chỉ thay trạng thái bán của chi nhánh được phép; Admin Chi nhánh không sửa giá/danh mục chung. Đối chiếu trạng thái cuối đã khôi phục. Bỏ thao tác đổi trạng thái nếu không có món được chỉ định. |
| UAT-11 · Admin Chi nhánh / Employee có quyền kho | Mở **Kho**, xem tồn, ngưỡng và **Lịch sử nhập xuất**; mở một phiếu để xem trước/sau. Nếu điều phối viên chỉ định **nguyên liệu demo riêng**, nhập thử 1 đơn vị qua **Nhập kho**, ghi lý do, xem mã phiếu và số dư mới; có thể xuất hoàn lại 1 đơn vị nếu hợp lệ. | Tồn thay đổi đúng số lượng, lịch sử có phiếu/lý do; không cho xuất làm âm kho. Bán hàng không tự trừ nguyên liệu trong MVP. Nếu không có fixture riêng, chỉ kiểm đọc và ghi `Not Run` cho phần ghi kho. |
| UAT-12 · Owner | Đăng nhập tài khoản Owner do điều phối viên cấp. Mở **Tổng quan**, chọn khoảng ngày chứa đơn UAT và bấm **Xem kết quả**. Xem toàn chuỗi, rồi lọc từng chi nhánh. | Thấy doanh thu/đơn của các chi nhánh trong quyền Owner. Chỉ đơn **Hoàn tất** (COMPLETED) được tính; **Đã hủy** (CANCELLED) và **Cần xem lại** (NEEDS_REVIEW) không tính. Dữ liệu cũ có thể làm tổng lớn hơn giá trị ba đơn mới; so sánh cùng bộ lọc/thời điểm. |
| UAT-13 · System Admin | Đăng nhập tài khoản System Admin được cấp, mở danh mục/menu quản trị và danh sách chi nhánh/nhân viên theo quyền. Xem một món đang mở và một món tạm ngưng nếu có. | Thấy chức năng quản trị toàn hệ thống. Không thay giá, tài khoản hoặc trạng thái chung trên dữ liệu dùng chung chỉ để lấy ảnh UAT; phần ghi chỉ chạy với fixture được giao. |
| UAT-14 · tất cả vai trò | Mở một trang yêu cầu đăng nhập sau khi **Đăng xuất**; trên điện thoại mở **Menu**/thanh điều hướng dưới để tìm chức năng. | Trang bảo vệ yêu cầu đăng nhập lại; điều hướng mobile hoạt động, bảng nhiều cột có thể kéo ngang. |

Nếu biểu mẫu đăng nhập sau khi phiên hết hạn hiện lỗi hoặc bị chặn ở lần đăng nhập đầu, ghi `Fail` và liên kết [SCRUM-140](https://khangba153.atlassian.net/browse/SCRUM-140) nếu tái hiện đúng hiện tượng; tải lại `/login` có thể dùng để tiếp tục các ca khác, nhưng **không đổi kết quả ca lỗi thành Pass**.

## 5. Mẫu ghi kết quả và báo lỗi

Sao chép mẫu này vào bình luận Jira SCRUM-131 cho **từng người tham gia** hoặc vào biên bản có tên người xác nhận:

```text
Người UAT: <họ tên / tài khoản Jira>
Thời gian (VN): <dd/mm/yyyy hh:mm>
Web / phiên bản được xác nhận: <URL, SHA hoặc mã build>
Thiết bị / trình duyệt: <ví dụ: Windows 11 / Chrome>
Vai trò / chi nhánh / tên đăng nhập demo: <không ghi mật khẩu>
Ca đã chạy: UAT-01, UAT-02, UAT-03, UAT-04, UAT-05
Món / size / topping / số ly / tổng dự kiến: <...>
Mã đơn / trạng thái / tổng thực tế: <...>
Kết quả từng ca: <Pass | Fail | Blocked | Not Run; nêu khác biệt nếu có>
Nhận xét sử dụng của tôi: <một hoặc hai câu>
Liên kết ảnh/biên bản: <...>
Tôi xác nhận chính mình đã thực hiện lượt này: <Có / Không>
```

Khi phát hiện lỗi, gửi QA: **mã ca, URL/bản build, vai trò/chi nhánh, giờ, dữ liệu đầu vào, các bước từ đầu, kết quả mong đợi, kết quả thực tế, tần suất tái hiện, ảnh/video đã che bí mật, mức ảnh hưởng**. QA kiểm tra trùng rồi lập/liên kết issue Jira, gắn lỗi vào dòng UAT tương ứng. Ví dụ: “UAT-04, Employee CN01, mã BF-…, 10:25; màn tạo báo thành công 35.000đ nhưng Đơn của tôi không tìm thấy theo mã/ngày 17/11; thử lại 2/2 lần.” Không dùng câu chung chung như “không chạy”.

## 6. Điều kiện kết thúc và bàn giao

- Ba hàng trong [biên bản SCRUM-131](../tests/sprint5/QA-SCRUM-131-UAT-Handoff-2026-11-17.md) có **ba người thật**, giờ thực tế, **ba mã đơn họ tự tạo**, expected/actual, kết quả và liên kết xác nhận do chính họ gửi.
- Các ca theo vai trò được ghi `Pass`/`Fail`/`Blocked`/`Not Run` trung thực; lỗi còn mở có mã Jira và người phụ trách. QA không dùng kết quả smoke kỹ thuật của mình thay cho xác nhận PCG.
- Khang/điều phối viên xác nhận đúng bản phát hành đã kiểm và quyết định: **chấp nhận**, **chấp nhận có điều kiện** (nêu lỗi/giới hạn), hoặc **chưa chấp nhận**. Ghi người quyết định, ngày giờ và liên kết xác nhận. Chỉ chuyển SCRUM-131 sang Done khi tiêu chí của issue và bằng chứng được đáp ứng.

**Tham khảo thao tác:** [Hướng dẫn Employee/Admin Chi nhánh](Sprint5-User-Guide.md) và [biên bản UAT chưa điền](../tests/sprint5/QA-SCRUM-131-UAT-Handoff-2026-11-17.md). Không dùng các trang `/preview/*` làm bằng chứng chức năng thật.
