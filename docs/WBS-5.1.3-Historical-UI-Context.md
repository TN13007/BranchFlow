# Nguồn ảnh kho lịch sử — dùng lại cho WBS 5.1.3

Ngày lập chỉ mục: 11/11/2026. Ảnh và lượt quan sát gốc: 10/11/2026. Không phải lượt chụp/test mới.

Nguồn báo cáo local: D:/QLDA/BranchFlow/docs/Sprint5-UAT-Responsive.md. Trích phần liên quan dưới đây; không đưa toàn bộ báo cáo WBS khác vào commit.

## Ngữ cảnh gốc

Người thực hiện: Công Thành, với công cụ hỗ trợ. Ngày thực hiện: 10/11/2026. WBS 4.4.1, 4.6.2, 4.6.5. Nhánh `thanhnc`, nền `47541ee`, thay đổi đang ở working tree, chưa commit/PR. Đây là tự kiểm tra local và kịch bản bàn giao cho Hòa Thuận/Khang; chưa phải biên bản người dùng ký nghiệm thu.
Web HTTPS localhost:17132 → API HTTPS localhost:17107 → SQL Server `BranchFlow_Test`. Các thao tác ghi của mục 3 chỉ vào Test. Chrome Windows 156.0.8078.81; Edge Windows 157.0.4341.42. Responsive bằng viewport override 360/768/1366 × 800, không phải thiết bị Android. Edge dùng cửa sổ desktop mặc định, không gán kích thước emulation.
| U02 / Owner | Mở kho; xem tồn, cảnh báo, lịch sử; không có nút ghi | WBS5 CN01: 7 lít, ngưỡng 8, dưới ngưỡng; có IN/OUT, không có form ghi | Pass |


## Phạm vi sử dụng

Ảnh owner-stock-1366.png gốc đã được xem trực tiếp: CN01, nguyên liệu WBS5 - Cốt trà đào, tồn 7 lít, ngưỡng 8, cảnh báo Dưới ngưỡng; lịch sử có IN #12 / OUT #13. Owner không có form ghi. Ảnh còn chứa các nguyên liệu/phiếu FE khác, không nhận đó là fixture BF-WBS5-v1.

Báo cáo gốc ghi nền 47541ee + working tree, chưa có SHA build chính xác cho ảnh kho. Chỉ dùng như bằng chứng quan sát lịch sử trên Test; không gán ảnh thành kiểm UI main hiện tại. Kết quả API/SQL có provenance commit riêng trong chỉ mục WBS-5.1.3.

Bản ảnh được kèm tại docs/evidence/WBS-5.1.3/owner-stock-1366-reused-2026-11-10.png. SHA256 ảnh gốc/bản kèm:
B0B90FA2FA7647C70EDCF42C7A9238AC6D38C49C40BFFB5591E4765292C734BB
