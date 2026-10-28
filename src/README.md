# `src/` – Định hướng mã nguồn BranchFlow

Trong **Tuần 1**, thư mục này được tạo để chuẩn bị lưu trữ mã nguồn; **chưa bắt buộc triển khai chức năng**. Không coi README này là bằng chứng hệ thống đã chạy.

## 1. Công nghệ dự kiến

- Backend: ASP.NET Core.
- Frontend: Blazor, Bootstrap.
- Database: SQL Server.
- Mô hình: Website responsive phục vụ Owner và Employee mobile-first, Admin desktop-first.

## 2. Phạm vi phát triển

| Phân hệ | Nội dung chính | Vai trò liên quan |
|---|---|---|
| Xác thực/phân quyền | Đăng nhập, role, giới hạn truy cập dữ liệu theo chi nhánh | Owner, Admin, Employee |
| Chi nhánh/nhân viên | Thông tin chi nhánh, nhân sự và phân công | Admin, Owner xem |
| Menu | Sản phẩm, danh mục, size/giá, topping, trạng thái bán theo chi nhánh | Admin, Employee sử dụng |
| Order | Chọn món, size, topping/note/số lượng, giỏ hàng, xác nhận order | Employee, Admin xử lý sai |
| Kho | Nhập, xuất, kiểm kê và cảnh báo tồn thấp | Admin, Employee theo quyền |
| Dashboard | Doanh thu, sản phẩm/đơn và dữ liệu theo chi nhánh | Owner, Admin |
| Bảng lương | Admin nhập kỳ lương; nhân viên xem lương cá nhân | Admin, Owner xem, Employee xem bản thân |

## 3. Quy trình order MVP (thống nhất sơ bộ)

1. Employee chọn sản phẩm.
2. **Bắt buộc** chọn size tương ứng với giá.
3. Có thể thêm topping, note hoặc thay số lượng; số lượng mặc định 1 là đề xuất UX cần chốt.
4. Xác nhận cấu hình món để đưa vào giỏ hàng.
5. Mở giỏ hàng, xem lại và xác nhận tạo order.
6. Lưu order; không yêu cầu lựa chọn hình thức thanh toán và không tích hợp cổng thanh toán.

Các quy tắc trạng thái order/hủy/điều chỉnh sẽ được đặc tả ở Sprint phân tích; không tự nhận đã chốt chi tiết.

## 4. Quy ước code dự kiến

- Tách nhiệm vụ giữa giao diện, nghiệp vụ và truy cập dữ liệu; kiến trúc solution cụ thể chốt sau khi thiết kế.
- Có kiểm tra authorization trên server; không dựa duy nhất vào việc ẩn nút trên UI.
- Cập nhật database/schema có migration hoặc script được kiểm soát, tránh ghi đè dữ liệu ngoài ý muốn.
- Tuyệt đối không commit secret, connection string chứa mật khẩu hoặc dữ liệu sản xuất.
- Mã nguồn và các thay đổi quan trọng phải có commit/PR rõ ràng, liên kết Jira.

## 5. Cần thực hiện sau Tuần 1

- [ ] Thống nhất cấu trúc solution thực tế.
- [ ] Thiết kế ERD và schema theo yêu cầu đã chốt.
- [ ] Lập API contract/luồng Blazor; chốt auth và role/branch scope.
- [ ] Thiết lập chạy Development/Test và bộ dữ liệu mẫu.
- [ ] Tích hợp/kiểm thử từng phân hệ theo Sprint.

**Đầu mối định hướng kỹ thuật:** Các Developer phối hợp PM/BA; thiết kế chính thức phải được nhóm review.