# Kết quả kiểm tra bản tích hợp ngày 19/11/2026

## Phiên bản và môi trường

- Candidate ứng dụng: `05c6b165132809958c69929b95efac900925043b`, nhánh `khang`.
- Tích hợp PR #76 của Thuận và PR #77 của Thành; giữ phần giao diện và báo sai đơn của bản local.
- Windows, .NET SDK 10.0.300; build Release. SQL integration dùng `BranchFlow_Test`; browser dùng `BranchFlow_UxReview` trên SQL Server local. Các phép thử không tác động database cá nhân/production.
- Các lượt trình duyệt do công cụ tự động thực hiện theo ủy quyền PM; không đại diện cho người tham gia UAT và không thay biên bản nghiệm thu.

## Kết quả

| Nhóm kiểm tra | Kết quả và phạm vi | Minh chứng |
|---|---|---|
| Build Release | 0 lỗi, 0 cảnh báo ở lượt đóng gói thành công | build-release.log |
| .NET với SQL Server | 128 Passed, 0 Failed, 0 Skipped | test-release.log, integrated.trx |
| Giao diện Employee | Năm màn tại 360/390/1440 px; gọi món, giới hạn topping, sửa giỏ, tạo và tìm lại đơn BF-DEMO-01-20261119-0001 | browser-staff.log |
| Luồng xuyên vai trò | 11 kịch bản: đăng nhập, báo sai/trùng, chặn sai quyền, sửa số ly, menu, hủy/doanh thu, Owner, kho, quyền kho, đăng xuất | ket-qua-trinh-duyet.json, browser-roles.log |
| Danh mục và giá | Tạo/đọc lại bảy loại danh mục, đổi giá món từ 21.000 lên 24.000 đồng | browser-catalog.log |
| SCRUM-140 | API thật lưu đơn, proxy trả 401; nhấn đăng nhập lại và gửi form một lần thành công; giữ giỏ và chặn gửi trùng; không còn lỗi form hết hạn | relogin-result.json, hai ảnh relogin |
| Responsive/điều hướng Edge | 24 mục điều hướng x 3 viewport 360/768/1366 = 72 lượt đạt | navigation-msedge.json |
| Responsive/điều hướng Chrome | Cùng 72 lượt đạt | navigation-chrome.json |

Các dòng là những tập kiểm tra có phạm vi khác nhau; không cộng 144 lượt điều hướng vào số test case chức năng hoặc người dùng UAT. Hai script dùng tiền tố UAT kế thừa mã chạy cũ, nhưng đây là kết quả tự kiểm kỹ thuật.

## Ghi nhận trong quá trình thực thi

- Lượt trình duyệt đầu dừng ở đăng nhập vì fixture UxReview chưa dùng mật khẩu demo đã chốt. Chỉ đặt lại password hash của đúng chín tài khoản demo trong bản sao local; giữ trạng thái và phân công rồi chạy lại đạt.
- Lượt SCRUM-140 đầu dừng do locator khớp hai liên kết Đăng nhập lại. Thu hẹp locator vào giỏ hàng; lượt chạy lại đạt. Đây là lỗi chọn phần tử của script, không phải kết luận lỗi sản phẩm.
- Build đóng gói đầu bị các tiến trình API/Web kiểm thử giữ DLL. Dừng đúng các tiến trình đã mở cho lượt kiểm tra rồi chạy lại script đóng gói đạt.

## UAT nội bộ và giới hạn

UAT dự kiến thứ Bảy 21/11/2026: Khang điều phối, cùng Thành, Thuận, Thuần và Tài. Giờ thực hiện chưa chốt. Mỗi người tự tạo đơn riêng và xác nhận kết quả của mình. Kết quả và chữ ký hiện chưa có; SCRUM-131 và SCRUM-139 giữ mở.

Chưa thực hiện Chrome Android/Safari iOS trên thiết bị thật hoặc kiểm tải 10 người. Phiên bản Azure được ghi nhận riêng trong hồ sơ public sau khi deploy; không suy từ kết quả local sang public.
