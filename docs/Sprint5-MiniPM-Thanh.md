# Mini-PM Report — Nguyễn Công Thành

Em đã kiểm tra bổ sung gửi đơn hai lần, mất phiên/timeout sau gửi đơn, các form danh mục còn thiếu và sửa giá trên Edge. Full suite hiện đạt 81/81, không skip sau khi sửa clock hai test. Em ghi chi tiết và giới hạn tại [trích log retest 10/11](WBS-4.4.1/evidence/records/Source-L02-Excerpt.md); các ghi nhận trước đó bên dưới cần đọc theo thời điểm. Em vẫn chờ các đầu vào/phê duyệt từ nhóm và chưa tự chốt nghiệm thu.

WBS 5.6.3 • Ngày ghi nhận 10/11/2026 • Gửi Khang review • Nhánh thanhnc, nền 47541ee; thay đổi hiện chưa commit/push. Đây là báo cáo phần em thực hiện với công cụ hỗ trợ; em chưa xác nhận giờ thực tế hoặc ký nghiệm thu thay người khác.

## Phần em tiếp nhận và thực hiện

Em tiếp nhận core đã có đăng nhập, phân quyền, menu, đơn, kho và doanh thu. Em rà giao diện quản trị, bổ sung trạng thái tải/rỗng/lỗi, retry và giới hạn nút sửa theo vai trò. Em chỉnh hiển thị số tiền/số lượng và thông báo khi ghi kho chưa rõ kết quả. Trong kiểm tra responsive, em sửa ô phân công bị cắt và menu mobile quá rộng. Em không nhận toàn bộ backend/schema là phần mình tự viết.

Em bổ sung bộ dữ liệu demo có bốn đơn theo trạng thái, tồn 7/15/4 tại ba chi nhánh và doanh thu fixture 128.000đ. Script đã chạy lại trên Dev/Test, giữ tài khoản và mật khẩu cũ. Em chuẩn bị kịch bản bốn vai trò, hướng dẫn Admin Chi nhánh/Employee và hồ sơ bàn giao theo WBS.

## Kết quả em đã đối chiếu

Em đã kiểm tra các luồng tạo đơn 98.000đ, Admin đổi số ly thành 147.000đ rồi hủy có lý do/lịch sử; Employee thấy trạng thái mới. Kho đã nhập/xuất thật trên Test, chặn xuất vượt tồn và giữ tồn cuối. Khi dừng API Test, giao diện báo lỗi thay vì hiện doanh thu 0; bật API và thử lại thì khôi phục dữ liệu. Chrome có lượt bốn vai trò và ba kích thước; Edge mới smoke phần Owner.

Build Release đạt 0 lỗi/cảnh báo; 76 unit không Integration đạt. Em giữ riêng kết quả 62 kiểm tra FE API và 23 kiểm tra dữ liệu, không cộng chúng thành số ca QA. Hai lỗi integration do clock/assignment ở lượt mục 1 vẫn cần xử lý fixture và retest. SQL của hành trình mới đã khớp snapshot, lịch sử và tồn.

## Phần em còn cần phối hợp

- Em cần Thuần/Khang chốt đầu vào Owner đọc hồ sơ nhân viên; em chưa tự mở thêm quyền API.
- Em cần chạy lại ca nhấn xác nhận hai lần trên phiên mới; hiện chỉ mới chuẩn bị giỏ, chưa gửi.
- Em cần Hòa Thuận kiểm tra độc lập; các luồng ghi Edge, Android và smoke public chưa đủ minh chứng.
- Em cần báo cáo cá nhân của Khang, Tài, Thuần và xác nhận lại bản của Hòa Thuận trước khi gộp năm người. Em không viết thay đóng góp hoặc số giờ của các bạn.

## Tiến độ, rủi ro và bài học

Phần em được giao là 20 hoạt động/40 giờ kế hoạch, hạn nội bộ 13/11. Em chưa có số giờ thực tế đầy đủ nên không tự tính phần trăm hoàn thành hoặc khẳng định đủ giờ cho toàn bộ việc còn lại. Em sẽ ưu tiên các đầu vào chặn review, gửi rõ file và kết quả cho Khang rồi bổ sung sau retest.

Qua lượt này, em thấy chỉ kiểm tra document có tràn ngang hay không là chưa đủ: phần tử con vẫn có thể bị cắt. Em cũng cần dùng cùng bộ dữ liệu và cùng thời điểm khi đối chiếu doanh thu. Em giữ rõ phần đã có, phần em sửa và phần em tự kiểm tra để nhóm review được.

Minh chứng: [bàn giao FE responsive](WBS-4.6.2-Handoff.md), [dữ liệu demo kho và doanh thu](WBS-5.1.3-Handoff.md), [trích kết quả UAT 10/11](WBS-4.4.1/evidence/records/Source-L01-Excerpt.md), [kế hoạch](Sprint5-WBS-Sprint-Report.md). Em chờ Khang review trước khi chốt trạng thái và bàn giao bản chính thức.
