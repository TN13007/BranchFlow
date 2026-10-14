# `tests/` – Kế hoạch kiểm thử BranchFlow

**Trạng thái Tuần 1:** Đây là kế hoạch kiểm thử ban đầu; **chưa có test case được chạy hoặc kết quả Pass/Fail**. Danh sách sẽ được điều chỉnh khi yêu cầu và kiến trúc được chốt.

## 1. Mục tiêu và phạm vi

Mục tiêu là kiểm tra các luồng trọng yếu của 7 phân hệ MVP, đặc biệt **phân quyền theo vai trò, phân tách dữ liệu giữa chi nhánh, tạo order và cập nhật tồn kho**. Dữ liệu sử dụng trong test/demo là dữ liệu giả lập.

## 2. Danh sách kịch bản kiểm thử ban đầu

| ID | Nhóm | Kịch bản cần kiểm tra | Kết quả kỳ vọng |
|---|---|---|---|
| T-01 | Auth | Đăng nhập bằng tài khoản hợp lệ | Truy cập đúng chức năng vai trò. |
| T-02 | Auth | Truy cập nghiệp vụ Admin bằng Employee | Bị từ chối trên server/API. |
| T-03 | Owner | Owner thử ghi/sửa/xóa dữ liệu | Bị từ chối; Owner chỉ xem. |
| T-04 | Branch | Employee ở chi nhánh A yêu cầu dữ liệu riêng của B | Không đọc/sửa được dữ liệu ngoài quyền. |
| T-05 | Branch | Owner lọc báo cáo theo chi nhánh | Nhận đúng dữ liệu mẫu theo bộ lọc. |
| T-06 | Menu | Admin tạo/cập nhật sản phẩm/size/giá | Dữ liệu cập nhật đúng, đúng quyền. |
| T-07 | Menu | Employee chọn sản phẩm nhưng không chọn size | Không cho xác nhận món. |
| T-08 | Order | Chọn size, topping/note, thay quantity và thêm giỏ | Thành tiền, số lượng và giỏ hàng khớp thiết lập. |
| T-09 | Order | Xác nhận giỏ hàng để tạo order | Tạo một order hợp lệ, hiển thị trong lịch sử phù hợp. |
| T-10 | Order | Employee yêu cầu xem order của người khác | Bị chặn theo phân quyền đã đặc tả. |
| T-11 | Kho | Admin nhập kho chi nhánh A | Tồn kho A tăng, kho B không đổi. |
| T-12 | Kho | Employee xuất vượt tồn cho phép | Không phát sinh tồn âm. |
| T-13 | Kho | Hoàn thành kiểm kê chênh lệch | Cập nhật số lượng và lưu lịch sử nếu chức năng đã được chốt. |
| T-14 | Payroll | Admin nhập kỳ lương Employee A | Employee A xem được, Employee B không xem được. |
| T-15 | Dashboard | Tạo các đơn dữ liệu mẫu và tổng hợp doanh thu | Tổng doanh thu khớp quy tắc thống nhất, không tính đơn bị loại trừ. |
| T-16 | UI | Thao tác tạo order trên mobile | Không tràn màn hình, vùng chạm phù hợp. |
| T-17 | UI | Admin CRUD trên desktop | Giao diện và thao tác đúng theo phân quyền. |
| T-18 | Concurrent | Hai thao tác đồng thời cùng tác động dữ liệu kho/order | Không sinh trùng mã hoặc tồn sai theo yêu cầu đã chốt. |

## 3. Nguyên tắc đánh giá

- **Pass:** Kết quả thực tế khớp kết quả kỳ vọng và có log/ảnh/video/biên bản phù hợp.
- **Fail:** Có sự khác biệt; tạo bug Jira ghi bước tái hiện, môi trường, mức độ và người xử lý.
- **Blocked:** Chưa test được vì chưa có build/môi trường/dữ liệu cần thiết.
- **Not run:** Chưa thực hiện. **Toàn bộ danh sách trên hiện ở trạng thái dự kiến/Not run** cho đến khi nhóm cập nhật.

## 4. Định nghĩa hoàn thành kiểm thử MVP

Các test case được nhóm xác định là **trọng yếu** phải đạt 100% trước khi báo nghiệm thu; tỷ lệ này là **mục tiêu**, không phải kết quả Tuần 1. Các lỗi nghiêm trọng chưa giải quyết phải được phản ánh trung thực trong báo cáo.

## 5. Nhiệm vụ của QA / Documentation

- Chuẩn hóa testcase: ID, tiền điều kiện, dữ liệu, bước test, kết quả kỳ vọng, kết quả thực tế.
- Lập bug trên Jira, liên kết với issue/commit sửa lỗi.
- Lưu minh chứng test thật ở `tests/` hoặc nơi được nhóm thống nhất.
- Cập nhật test report và kết quả nghiệm thu sau khi chạy thật.

**Phụ trách chính:** Nguyễn Đặng Hòa Thuận (QA/Documentation, hỗ trợ Developer).
