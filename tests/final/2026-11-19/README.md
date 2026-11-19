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

Chưa thực hiện Chrome Android/Safari iOS trên thiết bị thật hoặc kiểm tải 10 người. Kết quả public được ghi riêng bên dưới, không suy từ kết quả local.

## Kiểm tra sau deploy lên Azure

Bản ứng dụng candidate `05c6b165132809958c69929b95efac900925043b` đã được đóng gói và deploy lên các App Service hiện có của nhóm ngày 19/11. Release manifest chỉ mô tả bước đóng gói nên trường `publicDeployment` trong manifest không ghi nhận bước triển khai sau đó.

- Web: https://branchflow-team-web-khang153.azurewebsites.net/login
- API: https://branchflow-team-api-khang153.azurewebsites.net
- Kudu API deployment `7d0723af-392f-4c98-948a-20c0665da812`: status 4, hoàn tất 12:26:55 UTC.
- Kudu Web deployment `60a13061-4e04-4680-94a0-33118d34f439`: status 4, hoàn tất 12:27:03 UTC.
- `public/public-smoke-20261119-123012.json`: 27 kiểm tra HTTP đạt. SHA triển khai do người vận hành đối chiếu manifest và lịch sử deploy, không phải endpoint tự chứng thực binary.
- `public/staff.log` và ba ảnh: năm màn Employee tại 360/390/1440 px; tìm món, topping, sửa giỏ, tạo và tìm lại đơn thật trên Azure.
- `public/order-cancel-revenue.json`: đơn thử `BF-DEMO-01-20261119-0001` trị giá 138.000 đồng; hủy bằng Admin sau lượt thử, doanh thu của chi nhánh ngày 19/11 từ 138.000 về 0. Không hủy các đơn khác.

Mã đơn có thể trùng với lượt local vì hai database độc lập. Lượt public này là kiểm tra kỹ thuật tự động của bản triển khai, không phải kết quả UAT người dùng ngày 21/11.
