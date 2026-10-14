# Project Charter – BranchFlow (v1.0)

**Nhóm:** 6 | **Thời gian:** 12/10/2026 – 30/11/2026 (50 ngày)  
**Trạng thái:** Đề xuất của nhóm, **chưa được giảng viên chấp thuận chính thức**.  
**Project Sponsor:** Chưa xác nhận. **Ngân sách tiền mặt dự kiến:** 0 VNĐ.

## 1. 16 thành phần của Project Charter

| STT | Thành phần | Nội dung |
|---|---|---|
| 1 | **Project Title** | Xây dựng hệ thống quản lý chuỗi cửa hàng đồ uống – **BranchFlow**. |
| 2 | **Background** | Dựa trên hoạt động bán mang đi tại Sâm thảo mộc Tuấn Kiệt (Quận 1). Một thành viên từng làm việc ở đây hai năm. Dự án học thuật lấy bối cảnh thực tế để **mô phỏng**, không do cửa hàng đặt hàng hay tài trợ. |
| 3 | **Business Need / Problem** | Hạn chế bỏ sót order truyền miệng; hỗ trợ ghi nhận và theo dõi kho; giảm tổng hợp thủ công; giúp nhân viên xem lương cá nhân. Mức tác động chưa được định lượng. |
| 4 | **Target Users** | **Owner:** chỉ xem, theo dõi toàn chi nhánh. **Admin:** quản lý các nghiệp vụ được phân công. **Employee:** tạo order, nghiệp vụ kho trong phạm vi quyền và xem lương bản thân. |
| 5 | **Project Goal** | Tạo website demo nội bộ responsive, quản lý dữ liệu theo chi nhánh và phân quyền ba vai trò, có thể trình diễn quy trình vận hành nghiệp vụ chính. |
| 6 | **Project Objectives** | Hoàn thành 7 phân hệ trong 50 ngày; demo tạo order trên mobile; phân quyền Owner/Admin/Employee; chạy kho, dashboard, lương bằng dữ liệu mẫu; kiểm thử các luồng trọng yếu; ưu tiên triển khai demo Internet với hạ tầng miễn phí. |
| 7 | **Major Deliverables** | Source và tài liệu GitHub; web demo; database/seed; đặc tả yêu cầu, thiết kế, WBS/Gantt, Sprint Plan và báo cáo; test cases/test report; biên bản demo/nghiệm thu học thuật và tổng kết. |
| 8 | **Initial Scope** | 7 phân hệ: (1) đăng nhập/phân quyền, (2) chi nhánh/nhân viên, (3) menu/size/topping/giá, (4) đơn hàng, (5) kho/kiểm kê, (6) doanh thu/dashboard, (7) bảng lương nhập thủ công và xem lương cá nhân. |
| 9 | **Out of Scope** | Ứng dụng Android/iOS native, POS thật, thanh toán trực tuyến, chatbot/AI, tích điểm/khuyến mãi, chấm công, tính lương tự động, tích hợp camera và vận hành production cho cửa hàng. |
| 10 | **Key Stakeholders** | Giảng viên, PCG, Nhóm 6 (PM/BA, 3 UI/UX–Developer, QA/Documentation), các vai trò Owner/Admin/Employee giả lập. Cửa hàng Tuấn Kiệt chỉ cung cấp bối cảnh từ trải nghiệm BA, chưa phải đối tác chính thức. |
| 11 | **Constraints** | Thời hạn 50 ngày; 5 sinh viên, mỗi người cam kết 12–15 giờ/tuần; ngân sách tiền mặt dự kiến 0 VNĐ; ASP.NET Core, Blazor, Bootstrap, SQL Server; công cụ Jira/ProjectLibre/GitHub; ràng buộc hosting miễn phí. |
| 12 | **Assumptions** | Quan sát quá khứ đủ làm cơ sở tình huống mô phỏng; dữ liệu giả lập sát luồng nghiệp vụ; nhóm có thể phối hợp đúng cam kết; hạ tầng miễn phí đủ demo hoặc có phương án dự phòng được thống nhất. |
| 13 | **High-Level Risks** | Quá tải phạm vi; chậm chốt yêu cầu; thành viên kiêm nhiệm; lỗi phân quyền/branch scope; lệch dữ liệu/đồng thời đơn hàng; hạ tầng miễn phí, dữ liệu demo không đủ hoặc thay đổi yêu cầu. |
| 14 | **Success Criteria** | Demo được 7 phân hệ và luồng nghiệp vụ trọng yếu; đúng phân quyền 3 vai trò và dữ liệu từng chi nhánh; mobile/desktop responsive; **100% test case trọng yếu đã được nhóm xác định phải đạt** trước nghiệm thu; hồ sơ/source được bàn giao đúng hạn. |
| 15 | **Project Manager** | **Bá Thành Chí Khang**, đồng thời phụ trách BA và hỗ trợ Developer; điều phối phạm vi, Sprint, nguồn lực, rủi ro và báo cáo. |
| 16 | **Sponsor / Authorization** | Sponsor chính thức chưa được xác nhận. PM đề xuất, nhóm thảo luận/thống nhất; Sprint Plan gửi PCG xem xét; đề tài và thay đổi vượt thẩm quyền trình giảng viên theo quy định. |

## 2. Phạm vi In Scope và Out of Scope

### Trong phạm vi

- **Một ứng dụng web responsive:** Owner và Employee ưu tiên mobile-first, Admin ưu tiên desktop-first.
- **Đa chi nhánh:** Menu/giá dùng chung theo thiết kế; trạng thái bán, đơn hàng, kho và doanh thu tách theo chi nhánh. Dùng **ba chi nhánh giả lập** trong demo để kiểm thử; chỉ một chi nhánh thật được dùng làm bối cảnh.
- **Bán hàng:** Employee chọn sản phẩm, size/giá bắt buộc; topping/note/số lượng là tùy chọn; thêm vào giỏ, xác nhận order. Không tích hợp thanh toán trực tuyến.
- **Kho:** Nhập, xuất, kiểm kê, theo dõi/cảnh báo tồn theo chi nhánh; dữ liệu demo.
- **Lương:** Admin nhập thông tin kỳ lương; Employee xem của mình; **không** tính tự động từ chấm công.
- **Quản trị:** Owner chỉ xem; Admin thao tác theo phân công; Employee thao tác giới hạn. Server phải kiểm soát quyền, không chỉ ẩn nút giao diện.

### Ngoài phạm vi

- Dùng dữ liệu sản xuất, triển khai phục vụ khách hàng thật, tích hợp POS hay hệ thống thương mại bên ngoài.
- Chương trình khuyến mãi/khách hàng thân thiết, chatbot AI, mobile app riêng, cổng thanh toán, chấm công và tự tính lương.
- Hệ thống mua hàng từ nhà cung cấp, tài chính kế toán đầy đủ hoặc triển khai SaaS cho nhiều doanh nghiệp.

## 3. Tiến độ cấp cao (dự kiến, cần thống nhất với PCG)

| Giai đoạn | Thời gian | Sản phẩm cần kiểm tra |
|---|---|---|
| Khởi động | 12–18/10 | Hồ sơ Tuần 1, Jira, GitHub và trình phê duyệt đề tài. |
| Phân tích/thiết kế | 19–25/10 | Yêu cầu, luồng nghiệp vụ, thiết kế sơ bộ, WBS/Gantt và kế hoạch nguồn lực. |
| Phát triển lõi | 26/10–15/11 | Phân quyền, chi nhánh, menu, order và kho, tích hợp tăng dần. |
| Hoàn thiện/kiểm thử | 16–23/11 | Dashboard, lương, test tích hợp, sửa lỗi. |
| Demo/nghiệm thu | 24–30/11 | Demo, test report, báo cáo và tài liệu bàn giao. |

**Quy ước Sprint:** Kế hoạch trên là khung, không thay thế Sprint Plan. PM gửi Sprint Plan cho PCG **trước buổi họp 1 ngày**; không tự mở rộng phạm vi đã chốt giữa Sprint khi chưa xử lý Change Request theo quy định.

## 4. Giải pháp rủi ro cấp cao

| Rủi ro | Dấu hiệu | Ứng phó sơ bộ |
|---|---|---|
| Phạm vi quá lớn | Task trễ nhiều qua các Sprint | Ưu tiên luồng demo cốt lõi, giới hạn độ sâu tính năng, báo PCG khi đề xuất điều chỉnh. |
| Không đồng nhất phân quyền | User thấy dữ liệu ngoài chi nhánh | Thiết kế test role/branch scope từ đầu, QA kiểm tra API và UI. |
| Chậm tích hợp | Tính năng riêng lẻ chạy nhưng không tích hợp | Tích hợp theo Sprint, review code và hợp đồng API. |
| Không deploy online miễn phí | Thiếu hosting/database phù hợp | Thử deploy sớm và trình phương án demo dự phòng. |
| Sai dữ liệu mẫu | Demo không chứng minh nghiệp vụ | Có kế hoạch seed và test case theo vai trò/chi nhánh. |

## 5. Các quyết định còn mở

- [ ] Giảng viên phê duyệt chính thức đề tài.
- [ ] Xác nhận Sponsor (nếu được lớp phân công) và quyền hạn cụ thể.
- [ ] PCG xác nhận lịch họp và Sprint Plan.
- [ ] Chốt môi trường hosting/đường dẫn demo và phương án dự phòng.
- [ ] Chốt chi tiết quy tắc trạng thái order, bảng lương và tiêu chí kiểm thử trong giai đoạn phân tích.

**Nguồn:** Báo cáo khởi động Tuần 1 Nhóm 6; `BaiTapNhom_Chuong1.pdf` và quy định đồ án nhóm môn Quản lý dự án CNTT.
