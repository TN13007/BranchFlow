# Nguồn thông tin và giả định cần kiểm chứng

**Trạng thái nguồn:** Dữ liệu định tính do một thành viên BA cung cấp từ trải nghiệm hai năm làm việc tại Sâm thảo mộc Tuấn Kiệt. **Không phải kết quả khảo sát độc lập**, không có tỷ lệ/số liệu thống kê.

## 1. Ma trận thông tin

| Mã | Nhận định | Nguồn/cơ sở | Mức tin cậy và giới hạn |
|---|---|---|---|
| E-01 | Cửa hàng bán mang đi, có một chi nhánh tại Quận 1. | BA từng làm việc tại đây. | Được BA xác nhận; cần xác minh nếu mô tả tình trạng hiện tại. |
| E-02 | Nhân viên nhận đơn bằng lời nói, nhân viên khác nhập POS. | Quan sát trực tiếp của BA. | Có cơ sở kinh nghiệm, chưa đo tần suất sai sót. |
| E-03 | Giờ đông khách có thể quên món/hỏi lại, khiến POS nhập thiếu. | Tình huống BA từng gặp. | Không có số liệu tổng số đơn sai. |
| E-04 | Kiểm kho bằng đếm/quan sát, báo hàng thiếu qua tin nhắn. | Quan sát trực tiếp của BA. | Chưa có báo cáo kiểm kê để đối chiếu. |
| E-05 | Việc kiểm kho có lúc phải chờ nhân viên ca khác. | Trải nghiệm trực tiếp của BA. | Không có thống kê thời gian chờ. |
| E-06 | Quản lý tổng hợp doanh thu và ghi sổ cuối ca. | Quan sát trực tiếp của BA. | Chưa được phép sử dụng số liệu doanh thu thực tế. |
| E-07 | Nhân viên chỉ biết bảng lương vào cuối kỳ, khó xem chi tiết trước đó. | Trải nghiệm của BA. | Chưa có xác nhận độc lập từ tất cả nhân viên. |
| E-08 | Owner và Admin là hai người khác nhau tại cửa hàng. | BA xác nhận. | Mô hình phân quyền BranchFlow vẫn chỉ là thiết kế cho hệ thống demo. |

## 2. Các giả định ban đầu

| Mã | Giả định | Ảnh hưởng nếu sai | Cách kiểm chứng đề xuất |
|---|---|---|---|
| A-01 | Quy trình từng quan sát vẫn gần với hiện trạng cửa hàng. | Mô hình nghiệp vụ có thể không còn đại diện. | Nếu có điều kiện và được cho phép, trao đổi với nhân viên/quản lý; nếu không, ghi rõ bối cảnh quá khứ. |
| A-02 | Employee có thể thao tác order nhanh trên điện thoại. | Mục tiêu UX có thể không đạt. | Kiểm thử tác vụ với dữ liệu mẫu; ghi nhận thời gian thao tác thực tế khi demo. |
| A-03 | Quản lý kho điện tử hỗ trợ giảm bỏ sót ghi nhận. | Kho vẫn có thể không chính xác nếu kiểm đếm đầu vào sai. | Test nhập/xuất/kiểm kê và đối chiếu tồn kho bằng dữ liệu mẫu. |
| A-04 | 5 người, 12–15 giờ/tuần/người đáp ứng 7 phân hệ trong 50 ngày. | Trễ tiến độ hoặc chất lượng thấp. | Phân rã WBS, ước lượng, theo dõi velocity và rủi ro từng Sprint. |
| A-05 | Dịch vụ miễn phí đủ để demo ASP.NET Core/Blazor/SQL Server trên Internet. | Không deploy online được. | Thử triển khai sớm; đề xuất phương án dự phòng xin chấp thuận. |
| A-06 | Dữ liệu giả lập đủ phản ánh kịch bản đa chi nhánh. | Không kiểm thử được phân quyền/phân tách dữ liệu. | Chuẩn bị seed tối thiểu ba chi nhánh giả lập, mỗi chi nhánh có user/order/kho riêng. |

## 3. Quy tắc sử dụng nguồn thông tin

1. Không mô tả dữ liệu này là kết quả phỏng vấn 3–5 người hoặc khảo sát có thống kê.
2. Không gắn tên nhân viên thật, thông tin khách hàng, số liệu kinh doanh hay ảnh camera chưa được cho phép.
3. Không tự điền phần trăm giảm lỗi, thời gian phục vụ hoặc lợi nhuận nếu chưa đo.
4. Phân biệt **quan sát trong quá khứ** và **thực trạng hiện tại**.
5. Ghi nhận kết quả kiểm thử và xác nhận nghiệp vụ mới bằng minh chứng khi thật sự thực hiện.

**Đầu mối giải thích nghiệp vụ:** Bá Thành Chí Khang (PM/BA). Các thành viên dùng tài liệu này để hiểu phạm vi, nhưng vẫn phải tự rà soát nội dung được giao.
