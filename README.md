# BranchFlow – Hệ thống quản lý chuỗi cửa hàng đồ uống

> **Đồ án môn Quản lý dự án CNTT – Nhóm 6, UEH**  
> **Trạng thái:** Đề tài đã được nhóm lựa chọn, **chưa được giảng viên phê duyệt**.  
> **Thời gian dự kiến:** 12/10/2026 – 30/11/2026 (50 ngày, tính cả hai ngày).  
> **Mục đích:** Xây dựng website **demo học thuật**, không triển khai chính thức tại cửa hàng.

## 1. Giới thiệu

BranchFlow là hệ thống web nội bộ hỗ trợ quản lý hoạt động kinh doanh đồ uống ở một hoặc nhiều chi nhánh. Đề tài lấy bối cảnh tham khảo từ **Sâm thảo mộc Tuấn Kiệt, Quận 1, TP.HCM**, nơi một thành viên trong nhóm từng làm việc hai năm. Cửa hàng được mô tả có một chi nhánh; các chi nhánh bổ sung trong bản demo là **dữ liệu giả lập**, không phải chi nhánh thật của Tuấn Kiệt.

Dự án được **Nhóm 6 phát triển mới từ đầu**. Cửa hàng không phải bên đặt hàng, tài trợ hay đơn vị chính thức nghiệm thu sản phẩm.

## 2. Vấn đề cần giải quyết

- Khi đông khách, việc nhận order bằng lời nói rồi truyền cho người nhập POS có thể dẫn đến quên hoặc sót món.
- Kiểm kê nguyên liệu chủ yếu bằng cách quan sát/đếm thủ công; hàng thiếu được báo cho quản lý qua tin nhắn.
- Doanh thu được quản lý tổng hợp và ghi sổ cuối ca.
- Nhân viên không chủ động xem được thông tin lương chi tiết trước kỳ trả lương.

Đây là thông tin từ **trải nghiệm làm việc trong quá khứ của thành viên BA**; nhóm chưa khảo sát độc lập hay đo lường tỷ lệ lỗi hiện tại. Hiệu quả của hệ thống là kỳ vọng cần được kiểm thử, **không phải kết quả đã chứng minh**.

## 3. Người dùng và trải nghiệm

| Vai trò | Quyền/nhu cầu chính | Ưu tiên giao diện |
|---|---|---|
| **Owner** | Xem báo cáo kinh doanh, đơn hàng, kho, nhân viên và bảng lương của các chi nhánh; **chỉ xem** | **Mobile-first** |
| **Admin** | Quản lý chi nhánh được phân công, nhân viên, menu, kho, bảng lương và trường hợp order cần xử lý | **Desktop-first** |
| **Employee** | Tạo order, theo dõi order của mình, xuất/kiểm kho theo quyền, xem lương cá nhân | **Mobile-first** |

Ứng dụng là **một website responsive**, không phải ứng dụng Android/iOS riêng.

## 4. Phạm vi MVP – 7 phân hệ

1. Đăng nhập và phân quyền theo vai trò/chi nhánh.
2. Quản lý chi nhánh và nhân viên.
3. Quản lý menu: danh mục, sản phẩm, size, topping và giá.
4. Quản lý bán hàng và đơn hàng.
5. Quản lý kho: nhập, xuất, kiểm kê và cảnh báo tồn thấp.
6. Dashboard/báo cáo doanh thu theo dữ liệu mẫu và chi nhánh.
7. Quản lý bảng lương cơ bản: Admin nhập, Employee xem lương cá nhân.

**Ngoài phạm vi MVP:** Ứng dụng native, thanh toán trực tuyến/cổng thanh toán, tích hợp POS thực tế, AI/chatbot, khuyến mãi/tích điểm, chấm công, tính lương tự động, quản lý nhà cung cấp và vận hành sản xuất tại cửa hàng.

## 5. Công nghệ và công cụ

- **Backend:** ASP.NET Core.
- **Frontend:** Blazor và Bootstrap (chi tiết cấu hình sẽ được thống nhất trong giai đoạn thiết kế).
- **Cơ sở dữ liệu:** SQL Server.
- **Quản lý công việc:** Jira (Scrum/Sprint).
- **Lập tiến độ:** ProjectLibre.
- **Mã nguồn và lịch sử đóng góp:** GitHub.
- **Trao đổi và tài liệu:** Zalo, Google Meet, Google Drive.
- **Ngân sách tiền mặt dự kiến:** 0 VNĐ, ưu tiên hạ tầng miễn phí; nếu không đáp ứng, đề xuất phương án demo dự phòng để nhóm/PCG xem xét.

## 6. Nhóm thực hiện

| Thành viên | Vai trò chính |
|---|---|
| Bá Thành Chí Khang | Project Manager (PM), Business Analyst (BA), hỗ trợ Developer |
| Nguyễn Anh Trường Tài | UI/UX Designer / Developer – sản phẩm, menu và bán hàng |
| Nguyễn Công Thành | UI/UX Designer / Developer – chi nhánh, nhân viên và phân quyền |
| Nguyễn Phước Thuần | UI/UX Designer / Developer – kho, bảng lương và báo cáo |
| Nguyễn Đặng Hòa Thuận | QA / Documentation, hỗ trợ Developer |

Mỗi thành viên đã xác nhận cam kết **12–15 giờ/tuần** theo thông tin PM cung cấp ngày 13/10/2026. Phân công task và tiến độ được cập nhật theo Jira thực tế.

## 7. Cấu trúc repository

```text
BranchFlow/
├── README.md
├── docs/
│   ├── topic-selection.md
│   ├── problem-statement.md
│   ├── evidence-assumptions.md
│   ├── project-charter.md
│   ├── stakeholder-register.md
│   ├── team-charter.md
│   ├── change-request-scenario.md
│   └── ai-usage-disclosure.md
├── planning/
│   ├── week-01-plan.md
│   ├── week-01-summary.md
│   └── contribution-plan.md
├── src/
│   └── README.md
└── tests/
    └── README.md
```

Trong **Tuần 1**, nội dung trọng tâm là hồ sơ khởi động, tổ chức công việc và repository; **chưa bắt buộc có mã nguồn nghiệp vụ hoặc kết quả kiểm thử**.

## 8. Tài liệu và liên kết

- [Bảng lựa chọn đề tài](docs/topic-selection.md)
- [Problem Statement](docs/problem-statement.md) – [Nguồn thông tin và giả định](docs/evidence-assumptions.md)
- [Project Charter](docs/project-charter.md)
- [Stakeholder Register](docs/stakeholder-register.md)
- [Team Charter](docs/team-charter.md)
- [Kế hoạch Tuần 1](planning/week-01-plan.md) – [Tổng kết Tuần 1](planning/week-01-summary.md)
- [Tình huống thay đổi yêu cầu](docs/change-request-scenario.md)
- [Phân công đóng góp GitHub](planning/contribution-plan.md)
- [Định hướng source](src/README.md) – [Định hướng kiểm thử](tests/README.md)
- [Khai báo sử dụng AI](docs/ai-usage-disclosure.md)
- **Jira:** https://khangba153.atlassian.net/jira/software/projects/SCRUM/boards/1 (người xem có thể cần được cấp quyền truy cập).
- **GitHub:** https://github.com/TN13007/BranchFlow

## 9. Tình trạng và nguyên tắc minh bạch

- Đã xác định đề tài, mục tiêu, cơ cấu nhóm và hướng triển khai sơ bộ.
- Đã tạo Jira Space/Sprint 1 và repository; PM cho biết đã thiết lập 10 task Tuần 1. Trạng thái cụ thể xem trên Jira.
- **Chưa có xác nhận giảng viên phê duyệt đề tài.** Project Sponsor chính thức chưa được xác nhận.
- Dữ liệu khảo sát, kết quả test, biên bản nghiệm thu và hiệu quả sau triển khai **chỉ bổ sung khi thực sự có minh chứng**.
- Mỗi thành viên phải đọc, chỉnh sửa, xác nhận phần mình chịu trách nhiệm và có đóng góp thực chất trên GitHub; việc tải nguyên bộ file mẫu lên một lần không thay thế yêu cầu này.
