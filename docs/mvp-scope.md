# Phạm vi BranchFlow MVP đến 26/11/2026

Phạm vi hiện hành của bản demo nhóm. Project Charter v1.0 lưu kế hoạch khởi động để đối chiếu lịch sử. [Kế hoạch nội bộ đã chốt](https://drive.google.com/file/d/11a5UynSuKC0JgacY4WzR-aK7wEbS8Uii/view) và [ProjectLibre](https://drive.google.com/file/d/17XlYiCPFriFhWqncNe0bsOus-KehSJwm/view) là nguồn theo dõi phạm vi và lịch hiện hành.

| Vai trò | Phạm vi bản demo |
|---|---|
| Owner | Xem chi nhánh, đơn, tồn kho và doanh thu toàn chuỗi |
| Admin Tổng | Quản lý chi nhánh, tài khoản, phân công, menu và giá chung. Quản lý kho, đơn và doanh thu toàn chuỗi |
| Admin Chi nhánh | Sửa hồ sơ nhân viên trong chi nhánh được giao. Bật/tắt món và topping tại chi nhánh. Quản lý kho, đơn và doanh thu trong phạm vi được giao |
| Employee | Tạo đơn và xem đơn của mình. Xem kho chi nhánh và nhập/xuất khi được cấp quyền |

Mỗi chi nhánh có tối đa một quản lý đang hiệu lực. Admin Tổng phân công ban đầu và có thể giao nhiều chi nhánh cho cùng một Admin Chi nhánh. Nhân viên đang có phân công không chuyển sang chi nhánh khác trong MVP.

Mỗi ly chọn tối đa ba phần cho từng topping. Giá tạo đơn lấy từ DB. Admin sửa số ly theo giá, size và topping đã lưu. Sửa/hủy yêu cầu lý do và lưu lịch sử. Đơn hủy không mở lại và không tính doanh thu. Doanh thu theo ngày Việt Nam và chi nhánh.

Kho dùng nhập/xuất thủ công. Số tồn và lịch sử giao dịch được lưu cùng nhau, chặn tồn âm. Bán hàng không tự trừ kho.

Hoãn bảng lương, điều chuyển, kiểm kê/duyệt phiếu, báo sai nhiều bước, dashboard nâng cao và kiểm tải mở rộng. Mốc kết thúc nội bộ 23h ngày 26/11/2026. Kết quả deploy public và nghiệm thu ghi theo môi trường và bằng chứng thực tế.

## Chạy bản MVP

Theo hướng dẫn HTTPS trong [Sprint 4 Demo Runbook](Sprint4-Demo-Runbook.md) để chạy API, Web và đăng nhập bốn vai trò. API quản trị dùng tiền tố `/api/management` và tái sử dụng schema Database First hiện có. [Menu mẫu tùy chọn](../database/README.md#menu-mẫu-từ-dự-án-cá-nhân) nạp trên DB local sau khi tạo các chi nhánh demo.

## Đóng gói demo

Từ root repo, tạo hai thư mục publish riêng:

```powershell
dotnet publish src/BranchFlow.Api -c Release -o "$env:TEMP/branchflow-mvp/api"
dotnet publish src/BranchFlow.Web -c Release -o "$env:TEMP/branchflow-mvp/web"
```

Host API cần SQL Server, `ConnectionStrings__BranchFlowDb`, `Jwt__Key`, `Jwt__Issuer` và `Jwt__Audience`. Host Web cần `Api__BaseUrl` trỏ đến API qua HTTPS. Cấu hình các giá trị trên host. Trước nghiệm thu public, mở Web bằng HTTPS và kiểm tra đăng nhập, menu, tạo đơn, tra cứu và truy cập ngoài chi nhánh bằng dữ liệu demo của môi trường đó.
