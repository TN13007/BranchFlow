# Trích báo cáo UI lịch sử — không phải lượt mới

Nguồn local: D:/QLDA/BranchFlow/docs/Sprint5-UAT-Responsive.md. Ngày trích 11/11/2026; ngày quan sát 10/11/2026.

Người thực hiện: Công Thành, với công cụ hỗ trợ. Ngày thực hiện: 10/11/2026. WBS 4.4.1, 4.6.2, 4.6.5. Nhánh `thanhnc`, nền `47541ee`, thay đổi đang ở working tree, chưa commit/PR. Đây là tự kiểm tra local và kịch bản bàn giao cho Hòa Thuận/Khang; chưa phải biên bản người dùng ký nghiệm thu.
Web HTTPS localhost:17132 → API HTTPS localhost:17107 → SQL Server `BranchFlow_Test`. Các thao tác ghi của mục 3 chỉ vào Test. Chrome Windows 156.0.8078.81; Edge Windows 157.0.4341.42. Responsive bằng viewport override 360/768/1366 × 800, không phải thiết bị Android. Edge dùng cửa sổ desktop mặc định, không gán kích thước emulation.
| U02 / Owner | Mở kho; xem tồn, cảnh báo, lịch sử; không có nút ghi | WBS5 CN01: 7 lít, ngưỡng 8, dưới ngưỡng; có IN/OUT, không có form ghi | Pass |
| U09 / Admin CN01 | Nhập 2 nguyên liệu FE-0d68e065 stock | Phiếu #18, tồn 7→9 g | Pass |
| U10 / Admin CN01 | Xuất 10 khi tồn 9 | Báo vượt tồn; SQL không có phiếu thất bại | Pass |
| U11 / Admin CN01 | Xuất hợp lệ 2 | Phiếu #19, tồn 9→7; mở lịch sử thấy trước/sau và -2 | Pass |

Ảnh owner-stock-1366.png: CN01 WBS5=7 lít, ngưỡng8, chỉ đọc. Ảnh branch-stock-1366.png: lịch sử phiếu OUT #19, 9→7 (-2) cho nguyên liệu FE riêng. Không nhận ảnh này là phiếu nhập10/xuất3 của test SQL mới.
Báo cáo gốc ghi 47541ee + working tree; exact SHA runtime chưa được pin. Ảnh/log gốc giữ ngày và nguồn, không gán thành ảnh của main af77b9e hiện tại.
Tương tác lỗi quá tồn có ghi nhận trong U10; ảnh được chọn không chụp đầy đủ thông báo lỗi, không nhận có ảnh xác nhận toàn bộ trạng thái.
