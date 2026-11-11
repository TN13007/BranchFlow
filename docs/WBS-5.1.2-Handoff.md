# WBS-5.1.2 — Menu và đơn mẫu theo trạng thái

Em là Công Thành, có công cụ hỗ trợ, thực hiện/tổng duyệt ngày **10/11/2026**. Jira **SCRUM-127**, StorySCRUM-107; mapping issue hiện có ghi5.1.2. Nhánh riêng **feat/SCRUM-127-wbs-5.1.2-demo-menu**.

## Đầu vào và phần em thực hiện

Em fetch main và tạo worktree từ **bf69ad0**; main này đã merge PR39/commitb7184c7 của WBS5.1.1. Vì vậy em tiếp nhận `tools/Seed-WbsDemo.ps1`, `database/003-demo-menu.sql`, `database/005-wbs-demo-data.sql` và verifier API có sẵn, không copy lại để nhận tác giả hoặc đưa code WBS khác vào commit5.1.2.

Em chạy seed rồi chạy lại trên **BranchFlow_Test**, bổ sung SQL manifest kiểm menu/đơn, lập bảng expected/actual và tổng duyệt UI/API/SQL mới trên build main sạch. Giữ tài khoản/chi nhánh/menu cũ; wrapper so fingerprint tài khoản trước/sau trong bộ nhớ, chỉ ghi Pass, không xuất hash/mật khẩu. Không chạy baseline, không reset DB, không đổi mật khẩu hoặc ghi Dev.

## Menu được dùng

Danh mục **Menu demo BranchFlow**:5món đang bán, mỗi món sizeM/L; L cao hơn M8.000đ. Thêm một món tạm ngưng để đối chiếu loại khỏi menu bán. Đây là phần menu demo, không phải toàn bộ các món đã có trong DB.

| Món | M | L |
|---|---:|---:|
| Trà đào cam sả | 30.000đ | 38.000đ |
| Trà vải | 42.000đ | 50.000đ |
| Cà phê sữa đá | 32.000đ | 40.000đ |
| Bạc xỉu | 38.000đ | 46.000đ |
| Matcha latte | 52.000đ | 60.000đ |

Món tạm ngưng(demo), sizeM25.000đ, giữ bản ghi nhưng IsAvailable=false tại ba chi nhánh. Topping dùng cho fixture:Trân châu5.000đ/phần, Thạch4.000đ/phần. Bộ **62món là tùy chọn**, lượt này không chạy004, không nhận đã seed62món. UI toàn menu CN01 hiện11món do có dữ liệu cũ; lọc danh mục demo còn đúng5món đang bán.

## Fixture / expected / actual trên Test

| Case | Mã đơn / ID trên Test | Branch / người tạo | Cấu hình | Trạng thái / tổng kỳ vọng và actual |
|---|---|---|---|---|
| completed-a | BF-DEMO-01-20261110-0006 /20 | CN01 /demo.employee.a | M,2ly; mỗi ly3Trân châu+1Thạch | COMPLETED /98.000đ — khớp UI/API/SQL |
| cancelled-a | BF-DEMO-01-20261110-0007 /21 | CN01 /demo.employee.a | M,3ly; topping như trên | CANCELLED /147.000đ — khớp UI/API/SQL |
| review-a | BF-DEMO-01-20261110-0008 /22 | CN01 /demo.employee.a | M,1ly; topping như trên | NEEDS_REVIEW /49.000đ — khớp UI/API/SQL |
| completed-b | BF-DEMO-02-20261110-0001 /23 | CN02 /demo.employee.b | M,1ly,không topping | COMPLETED /30.000đ — khớp UI/API/SQL |

Đơn giá cấu hình có topping=30.000+3×5.000+4.000=49.000đ/ly. Doanh thu **riêng fixture**=98.000+30.000=128.000đ; hai trạng thái còn lại không tính. Không dùng128.000đ làm tổng của cả DB có đơn khác.

Các trạng thái được tạo trực tiếp bằng script dữ liệu, có audit BF-WBS5-v1, không phải kết quả UAT một workflow báo sai/phê duyệt. Đơn NEEDS_REVIEW có lý do ghi rõ fixture. Không giả lịch sử OrderAdjustment cho đơn seed; muốn demo thao tác sửa/hủy thật thì tạo đơn riêng và dùng kịch bản WBS3.4.2.1. ID/mã trên Dev có thể khác; tra audit thay vì hard-code vào ứng dụng.

## Môi trường, thao tác và kết quả

Candidate ứng dụng **bf69ad0** (main đã fetch), không có thay đổi ứng dụng trong nhánh này. Web **https://localhost:17232** → API **https://localhost:17217**, SQLlocalhost/BranchFlow_Test; .NET10 Release; Chrome Windows. Tất cả lượt dưới do em thao tác ngày10/11/2026, không thay QA độc lập.

| Kiểm chứng | Expected | Actual / kết quả |
|---|---|---|
| Seed Test và chạy lại | Giữ ID/mã/trạng thái/giá; không nhân đôi, không đổi tài khoản | Cả hai lượt giữ20/21/22/23,4đơn/6phiếu; fingerprint khớp — Pass |
| Đăng nhập API4role | Mật khẩu hiện có, đúng role | Owner/Admin Tổng/Admin CN/Employee đúng;23assert API/SQL đạt — Pass |
| Menu SQL | 10giá M/L,5món mở tại3branch,1món ngừng tại3branch | SQLmanifest khớp — Pass |
| Fixture API/SQL | Đúng4trạng thái/cấu hình/tổng theo bảng | Bốn snapshot khớp; EmployeeA thấy3đơn của mình; scope AdminCN01 đúng — Pass |
| UI menu EmployeeA | Toàn menu có dữ liệu cũ, lọc demo5món; đúngsize/giá | 11món toàn menu,5món khi lọc; dialog M30.000/L38.000 — Pass |
| UI preview cấu hình | M,3Trân châu,1Thạch,2ly=98.000đ | Chọn bằng nút thật; preview98.000đ; đóng dialog, không gửi thêm đơn — Pass |
| UI trạng thái CN01 | Mã0006/0007/0008 và các tổng đúng | Mở từng chi tiết, thấy Hoàn tất/Đã hủy/Cần xem lại cùng snapshot — Pass |
| UI CN02 | EmployeeB chỉ có đơn mẫu của mình | Đăng nhập thật, mở ID23,1ly30.000đ không topping — Pass |
| Build Web Release | Biên dịch thành công | 0warning/error — Pass |

Lần nhập mật khẩu do người dùng cung cấp ban đầu bị API401; không tính lượt đó là Pass và không reset tài khoản. Em tìm lại thông tin đã dùng trong lịch sử của chính phiên làm việc, sau đó đăng nhập/kiểm chứng thành công; không đưa thông tin đăng nhập vào Git/log/Jira. Các lượt API và UI trong bảng là lượt thành công sau bước đó.

Ảnh preview chụp phần cuối dialog có số ly/tổng; cấu hình size/topping được đối chiếu bằng trạng thái UI quan sát và ảnh chi tiết đơn, không nhận một ảnh là chụp đủ mọi tùy chọn. Đã đăng xuất EmployeeA/B sau tổng duyệt, không lưu mật khẩu trình duyệt. API verifier logout các phiên riêng sau kiểm.

## Chạy lại và bàn giao

Từ root nhánh đã tích hợp main và đầu vào5.1.1:

```powershell
./tools/Seed-WbsDemo.ps1 -Database BranchFlow_Test
./tools/Seed-WbsDemo.ps1 -Database BranchFlow_Test
sqlcmd -S localhost -E -C -b -f 65001 -d BranchFlow_Test -W -i tests/sprint5/WBS-5.1.2/Verify-MenuOrders.sql
# API đã cấu hình đúng DB và mật khẩu cấp riêng trong env BRANCHFLOW_DEMO_PASSWORD:
./tests/sprint5/Verify-WbsDemoData.ps1 -Database BranchFlow_Test -BaseUrl https://localhost:17217
```

Người nhận cần cấu hình connection string/JWT ngoài Git để khởi động host; port17217/17232 là port dùng trong lượt này, không phải dịch vụ public cố định. Không chạy lại baseline hoặc đổi mật khẩu để làm verifier đạt. Wrapper còn kiểm fixture kho dùng chung từ5.1.1; đây không phải nghiệm thu thêm WBS5.1.3.

Em bàn giao Thuận/Khang dữ liệu, script chạy lại, manifest và luồng: EmployeeA xem menu/tùy chọn → tra3trạng thái → EmployeeB xem đơnCN02 → Owner/Admin đối chiếu API/SQL. Đầu ra tiếp5.1.3 của Thành có thể dùng lại bộ dữ liệu này. Chờ Khang review/Thuận QA retest/tổng duyệt nhóm; chưa có public, không ký thay và không tự Done cả SCRUM-127/Story.

Minh chứng: ảnh mới tại `docs/evidence/WBS-5.1.2/`; seed/rerun/API/SQL/build và phiếu tổng duyệt tại `tests/sprint5/WBS-5.1.2/`. Link commit cụ thể được gắn trong comment Jira, không dùng SHA tài liệu thay SHA ứng dụng đã chạy.
