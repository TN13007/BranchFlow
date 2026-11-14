# Dữ liệu demo mở rộng

Nguồn menu là dữ liệu seed Tuấn Kiệt đã chuyển đổi trong `004-personal-menu-demo.sql`. Không lấy người dùng, mật khẩu, thông tin khách hàng hoặc lịch sử giao dịch thật của dự án cá nhân. Nguyên liệu và toàn bộ giao dịch trong `006-expanded-demo.sql` là dữ liệu tổng hợp cho trình diễn.

## Bộ bổ sung

- 12 nguyên liệu: trà lài/đen/ô long, matcha, cà phê, bột sữa, đường, trân châu, thạch, sữa tươi, syrup đào/vải. Dùng kg/lít đúng đơn vị reference hiện có.
- 36 số dư kho ở ba chi nhánh; 12 phiếu nhập/xuất, 144 dòng lịch sử với before/change/after khớp. Mỗi chi nhánh có nguyên liệu hết, dưới ngưỡng và bình thường. Bán hàng không tự trừ kho, đúng scope MVP.
- 180 đơn trong 30 ngày tính theo ngày Việt Nam ở lần seed đầu tiên; 90 đơn ở CN01 và 90 ở CN02. Tổng 240 dòng món, chọn giá/size/khả dụng hiện có, topping 1–3 phần và ghi chú theo nhóm.
- 162 đơn hoàn thành, 18 đơn hủy; 18 đơn điều chỉnh số ly. Tổng 36 bản ghi lịch sử với snapshot trước/sau và lý do ghi rõ giả lập.
- CN03 không tạo đơn vì chưa có Employee đang hoạt động được phân công: dùng để demo doanh thu trống, không giả mạo người tạo đơn/phân công.
- Không thêm tài khoản hoặc đổi mật khẩu/phân công; không sửa menu/giá/khả dụng cũ. Không thêm UP SIZE 1300ML. Có thể demo tạo/sửa tài khoản trực tiếp bằng SystemAdmin.

Các ngày/phiếu/lịch sử giả lập không phải bằng chứng đã thực hiện workflow, kiểm thử hay công việc Jira/ProjectLibre. Muốn làm minh chứng nghiệm thu phải thao tác API/Web thật và ghi kết quả riêng.

## Chạy

Sau baseline/reference và DemoSeed, cần có menu mở bán ở CN01/02. Database máy Khang đã có 72 món/14 topping nên không cần nhập trùng 004.

```powershell
# Xem kết quả rồi rollback toàn bộ lượt thử, gồm counter cấp mã:
./tools/Seed-ExpandedDemo.ps1 -Server 'localhost\SQLEXPRESS' -Database BranchFlow_Dev
# Lưu dữ liệu:
./tools/Seed-ExpandedDemo.ps1 -Server 'localhost\SQLEXPRESS' -Database BranchFlow_Dev -Apply
# Kiểm tra chỉ đọc sau seed/thao tác demo:
sqlcmd -S 'localhost\SQLEXPRESS' -E -C -b -f 65001 -d BranchFlow_Dev -i database/verify-expanded-demo.sql
```

Chỉ hỗ trợ SQL local Dev/Test. Không tự nạp Test đang được QA dùng hoặc DB public. Khi mở SQL bằng SSMS, chọn đúng DB; mặc định `@Apply=0` là rollback, muốn lưu đổi thành 1. `sqlcmd -C` trong wrapper chỉ phục vụ chứng chỉ local.

Script dùng transaction, application lock và marker `BF-DEMO-EXPANDED-v1` ở AuditLog. Chạy lại không thêm đơn/nguyên liệu trùng, không khôi phục số dư/trạng thái demo đã được người dùng chỉnh. AnchorDate của lần đầu nằm trong marker; chạy lại ngày khác không tự dời lịch sử. Không xóa marker để cố chạy lại. Seed mới dùng phiên bản mới khi thực sự cần; không reset DB.

## Gợi ý demo trên máy Khang

Lần seed đầu 11/11/2026: lọc doanh thu **13/10–11/11/2026**. Chỉ phần đơn seed có doanh thu 18.587.000đ (CN01 9.295.000đ; CN02 9.292.000đ). Tổng giao diện có thể lớn hơn vì còn đơn cũ/người dùng tạo sau seed; đối chiếu cùng bộ lọc và thời điểm, không xóa đơn cũ để ép tổng.

1. Owner: xem toàn chuỗi → lọc CN01/CN02 → CN03 trống; kiểm đơn hủy không vào doanh thu.
2. BranchAdmin: xem phạm vi chi nhánh, mở đơn có nhiều dòng/topping/ghi chú, xem lịch sử; sửa số ly hoặc hủy một đơn hoàn thành với lý do mới để tạo minh chứng thật.
3. Employee A/B: xem đơn của mình và tạo thêm đơn qua menu thật.
4. Kho: chọn từng chi nhánh, xem Trà lài hết, Trà đen/Ô long dưới ngưỡng; xem lịch sử nhập/xuất rồi nhập thử bằng thao tác thật.
5. SystemAdmin: quản lý menu/nhân viên/chi nhánh trên dữ liệu hiện có; tạo tài khoản thử mới qua UI nếu cần, không nhân bản password hash.

Nếu hôm demo đổi ngày, trang tổng quan mặc định hôm nay có thể trống. Chọn lại khoảng theo AnchorDate. Các hình ảnh menu dùng URL đã có hoặc ô thay thế; bộ seed này không khẳng định ảnh ngoài Internet luôn truy cập được.

## Azure demo của nhóm

Sau khi DB nhóm đã có schema, reference data và tài khoản demo, chạy từ máy có quyền truy cập firewall SQL. Đặt mật khẩu SQL quản trị qua biến môi trường của tiến trình. Dùng `-Mode Apply` để nạp, `-Mode Verify` để kiểm tra. Kết nối mã hóa và runner kiểm tra cả tên database lẫn logical server. Các đơn đã có vẫn được giữ lại.

```powershell
./tools/Seed-AzureGroupDemo.ps1 -Mode Apply
./tools/Seed-AzureGroupDemo.ps1 -Mode Verify
```
