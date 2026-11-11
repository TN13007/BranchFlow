# WBS-5.2.3 — Viết User Guide cho Admin Chi nhánh/Employee

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, đối chiếu với build thật và bàn giao **11/11/2026**. Ngày 20/11 trong lịch là kế hoạch. Jira đúng WBS: **SCRUM-127** (Story **SCRUM-107**). Nhánh riêng: `docs/SCRUM-127-wbs-5.2.3-user-guide` tách từ `origin/main` (**ae595fd**).

## Phần tiếp nhận và phần em đóng gói

Em tiếp nhận nguyên vẹn tài liệu hướng dẫn người dùng **`docs/Sprint5-User-Guide.md`** từ working tree `D:\QLDA\BranchFlow\docs\Sprint5-User-Guide.md` (trùng khớp byte-for-byte SHA256 `F02DD8C80E7FF98C729A44D95D9FD8E28A526DD066F954DD07C49967C489167E`, Git blob `baf2a5b23ed102206bf9926248decf8d1d8f9caf`), kèm ảnh nhúng `docs/evidence/sprint5-uat/employee-cart-360.png` và bộ 9 ảnh chụp màn hình nguyên bản tại `docs/evidence/WBS-5.2.3/` minh họa đầy đủ các bước của hai vai trò **Employee** và **Admin Chi nhánh**.

Nội dung hướng dẫn được đối chiếu trực tiếp với UI thật và các lượt chạy UAT ngày `10/11/2026`:
1. **Đăng nhập & bảo mật**: Hướng dẫn vào `/login` (`https://localhost:7132/login` cho Dev hoặc `17132` cho Test), dùng tài khoản demo `demo.branchadmin.01`, `demo.employee.a`; tuyệt đối không công khai mật khẩu hoặc secrets trong tài liệu/ảnh chụp.
2. **Employee — Tạo đơn hàng (`/orders/create`)**: Kiểm tra chi nhánh được phân công, tìm/lọc danh mục, bấm `Chọn món`, chọn đúng 1 size, tối đa 3 phần cho mỗi loại topping trên mỗi ly, tối đa 1 ghi chú mỗi nhóm; ví dụ tính tiền thật (`M 30.000đ + 3 Trân châu 5.000đ + 1 Thạch 4.000đ = 49.000đ/ly`, 2 ly = `98.000đ`), bấm `Thêm vào đơn`, kiểm giỏ và bấm `Xác nhận tạo đơn` một lần; hướng dẫn xử lý khi mất phiên/timeout sau khi bấm gửi (kiểm tra `Đơn của tôi` trước khi tạo lại).
3. **Employee — Tra cứu và xem chi tiết đơn (`/orders/mine`, `/orders/{id}`)**: Lọc theo `Mã đơn hàng` và `Ngày kinh doanh`, bấm `Tìm đơn` / `Bỏ bộ lọc`; giải thích ý nghĩa 3 trạng thái `Hoàn tất` (`COMPLETED`), `Cần xem lại` (`NEEDS_REVIEW` — chỉ là dữ liệu mẫu, chưa mở quy trình báo sai nhiều bước), `Đã hủy` (`CANCELLED`); nêu rõ Employee chỉ xem đơn của mình trong phạm vi chi nhánh hiệu lực và không có nút sửa/hủy.
4. **Admin Chi nhánh — Quản lý trạng thái bán Menu**: Mở tab `Menu`, chọn chi nhánh, dùng nút `Mở bán` / `Ngừng bán` cho món và topping; nêu rõ Admin Chi nhánh không sửa được giá chung, size hoặc xóa món chung.
5. **Admin Chi nhánh — Sửa số ly hoặc hủy đơn**: Mở tab `Đơn hàng`, lọc ngày và bấm `Xem kết quả` → `Chi tiết`; chỉnh `Số ly` (tính lại theo giá snapshot đã chốt), nhập `Lý do sửa hoặc hủy` (bắt buộc 1–500 ký tự), bấm `Lưu số ly` hoặc `Hủy đơn` → `Xác nhận hủy đơn …`; đối chiếu `Lịch sử chỉnh sửa`.
6. **Kho cho Admin Chi nhánh & Employee được cấp quyền**: Xem tồn kho, ngưỡng cảnh báo và `Lịch sử nhập xuất`; bấm `Tải lại tồn kho và lịch sử`; chọn `Nhập kho` / `Xuất kho`, nhập số lượng dương (hỗ trợ phần lẻ, chặn xuất vượt tồn), nhập `Lý do` và bấm `Ghi giao dịch`; lưu ý bán hàng không tự trừ kho và MVP chưa có kiểm kê/duyệt phiếu; chỉ Admin mới có nút `Lưu ngưỡng`.
7. **Hồ sơ nhân viên & Doanh thu cho Admin Chi nhánh**: Mở `Nhân viên` → `Sửa hồ sơ` của Employee thuộc chi nhánh mình (không tạo tài khoản mới hay đổi vai trò/điều chuyển); mở `Tổng quan` lọc khoảng ngày (giờ VN) để xem doanh thu các đơn `COMPLETED`.
8. **Bảng xử lý lỗi thực tế & Đăng xuất**: Hướng dẫn xử lý 6 tình huống lỗi (mất kết nối API, sai/khóa tài khoản, rate-limit `429`, không có dữ liệu trong phạm vi, hết hạn phiên `401`, xung đột `RowVersion` `409`) và nhắc không dùng `/preview/*` làm minh chứng chức năng thật.

## Case / expected / actual

| Ca kiểm chứng | Expected | Actual / Nguồn | Kết quả |
|---|---|---|---|
| Đối chiếu tên màn hình, nhãn nút và thông báo lỗi với source Blazor trên `origin/main` | Khớp chính xác nhãn nút (`Chọn món`, `Thêm vào đơn`, `Xác nhận tạo đơn`, `Tìm đơn`, `Bỏ bộ lọc`, `Mở bán`/`Ngừng bán`, `Lưu số ly`, `Hủy đơn`, `Ghi giao dịch`, `Lưu ngưỡng`, `Sửa hồ sơ`, `Xem kết quả`) | Đối chiếu khớp với `OrderWorkspace.razor`, `Mine.razor`, `Detail.razor`, `ManagementMenu.razor`, `ManagementStock.razor`, `ManagementWorkspace.razor` | **Pass** (11/11) |
| Thử nghiệm theo từng bước hướng dẫn bằng tài khoản demo (`demo.employee.a`, `demo.branchadmin.01`) | Thực hiện trọn vẹn hành trình tạo đơn `98.000đ` -> sửa 3 ly `147.000đ` -> hủy đơn -> nhập/xuất kho `7->9->7` -> tra cứu doanh thu & lịch sử | Đã chạy thật trong kịch bản UAT `U07–U24` ngày `10/11/2026` (`Sprint5-UAT-Responsive.md`) và lưu 9 ảnh chụp tại `docs/evidence/WBS-5.2.3/` | **Pass** (dùng lại 10/11) |
| Kiểm tra không lộ mật khẩu / secrets và ghi rõ giới hạn phạm vi MVP | Không có mật khẩu/JWT key trong tài liệu và ảnh; nêu rõ phần hoãn (payroll, điều chuyển, kiểm kê, báo sai nhiều bước) | Đã kiểm tra toàn bộ `docs/Sprint5-User-Guide.md` | **Pass** |
| Đối chiếu với bản User Guide Owner/Admin Tổng (`5.2.2` của Khang) và gộp tài liệu hướng dẫn (`5.2.4` của Tài) | Đồng bộ thuật ngữ với `5.2.2` và bàn giao cho Trường Tài (`5.2.4`) | Chờ nhận bản `5.2.2` của Khang; bàn giao nhánh này cho Khang và Trường Tài (`5.2.4`) | **Pending / Chờ phối hợp** |

## Chưa đạt / phụ thuộc / người nhận

- **Phụ thuộc đầu vào**: Chờ bản `WBS 5.2.2` (*Viết User Guide cho Owner/Admin Tổng* — PM Khang phụ trách) để đồng bộ thuật ngữ chung.
- **Người nhận tiếp theo**: **Trường Tài (`WBS 5.2.4`)** nhận để tổng hợp hướng dẫn cài đặt, chạy thử và xử lý lỗi thường gặp; **PM Khang** review; **QA Hòa Thuận** hoặc thành viên độc lập chạy thử theo hướng dẫn trên bản tích hợp cuối.
