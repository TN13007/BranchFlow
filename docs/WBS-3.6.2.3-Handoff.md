# WBS-3.6.2.3 — Kết nối API, lọc ngày và chi nhánh theo quyền

Em là Công Thành, có công cụ hỗ trợ. Em thực hiện và kiểm UI/build ngày **10/11/2026**, chạy lại HTTP/SQL và hoàn tất bàn giao ngày **11/11/2026**, giờ Việt Nam. Đây là tự kiểm chứng local, chưa thay review của Khang hoặc QA của Thuận.

## Phạm vi và phiên bản

- Jira đã map đúng: **SCRUM-126**, WBS 3.6.2.3 và 3.6.2.4, Story SCRUM-105. Lượt này chỉ bàn giao **3.6.2.3**, không đóng cả issue/Story.
- Nhánh riêng: `feat/SCRUM-126-wbs-3.6.2.3-revenue`, ban đầu từ main `ae8c7f2bfd10d248d42b5def5f4348468d4e287d`. Ngày 11/11 em fetch và fast-forward lên main **dbfa03d6011608f81b07e806c071133a393b0fce**, build lại API/Web, chạy lại unit và HTTP/SQL trước commit; SHA commit bàn giao được gắn trong comment Jira.
- Em dùng lại `ManagementApiClient`, endpoint GET `/api/management/revenue`, `MvpService` và `MvpRepository` của core: lọc khoảng ngày VN sang UTC, chỉ COMPLETED, scope server, bảng Bootstrap. Em không nhận tác giả phần core này.
- Em chỉ sửa trạng thái hiển thị doanh thu trong `ManagementWorkspace.razor`: chờ lượt tải thành công mới hiện tổng/bảng; đang tải hoặc lỗi không hiện tổng 0 và thông báo trống gây hiểu nhầm. Loading có khóa bộ lọc và thông báo; lỗi vẫn dùng ApiError sẵn có. Không thêm chart, dashboard nâng cao, schema hoặc quyền mới.
- Build/source cụ thể tại `tests/sprint5/WBS-3.6.2.3/Build-Provenance.txt`; Git blob của component pin đúng phần ứng dụng đã tổng duyệt.

## Môi trường và dữ liệu

Windows, .NET 10 Release, Chrome; SQL `localhost/BranchFlow_Test`; API `https://localhost:17317`, Web `https://localhost:17332`. JWT/connection string được cấp ngoài Git. Dùng mật khẩu demo hiện có; không reset tài khoản, không ghi DB Dev. Port là host tạm phục vụ lượt kiểm, không phải URL public.

`Seed-RevenueBoundary.sql` chỉ cho phép BranchFlow_Test, thêm bốn header đơn phục vụ tổng hợp; chạy lại không nhân đôi/ghi đè dữ liệu đã đổi. Đây là fixture SQL trực tiếp, không phải bằng chứng luồng tạo đơn, chuyển trạng thái hoặc chi tiết size/topping. Giữ các dữ liệu demo cũ.

| ID / mã fixture | Chi nhánh | UTC CreatedAt | Giờ VN | Trạng thái | Tổng |
|---|---|---|---|---|---:|
| 35 / WBS3623-BEFORE | BF-DEMO-01 | 01/11/2026 16:59:59 | 01/11/2026 23:59:59 | COMPLETED | 100.000đ |
| 36 / WBS3623-MIDNIGHT | BF-DEMO-01 | 01/11/2026 17:00:00 | 02/11/2026 00:00:00 | COMPLETED | 200.000đ |
| 37 / WBS3623-CANCELLED | BF-DEMO-01 | 01/11/2026 17:30:00 | 02/11/2026 00:30:00 | CANCELLED | 999.000đ |
| 38 / WBS3623-BRANCH-B | BF-DEMO-02 | 01/11/2026 18:00:00 | 02/11/2026 01:00:00 | COMPLETED | 300.000đ |

CN01 dùng creator demo.employee.a; CN02 dùng demo.employee.b. Em đã chỉnh creator của header CN02 do chính lượt này tạo trước lần HTTP retest ngày 11/11; không đổi thời điểm/trạng thái/tổng. Seed sau đó chạy lại giữ ID 35–38. ID có thể khác trên máy khác; script nhận diện bằng mã fixture.

## Expected / actual và minh chứng

| Case đã chạy | Expected | Actual | Kết quả |
|---|---|---|---|
| Seed rồi chạy lại | 4 header, cùng ID/mã, không trùng | ID 35–38 giữ nguyên | Pass |
| Owner / SystemAdmin API 01–02/11 | Ba dòng ngày/branch, tổng 600.000đ | Trùng từng dòng SQL: CN01 ngày 01=100.000; ngày 02=200.000; CN02 ngày 02=300.000 | Pass |
| Owner UI chọn tất cả | 3 đơn, tổng 600.000đ, bảng đúng ngày VN | Khớp API/SQL, không có nút CRUD doanh thu | Pass |
| Owner UI chọn CN02 | 1 đơn, tổng 300.000đ | Chỉ dòng CN02 ngày 02/11 | Pass |
| Admin CN01 UI/API mặc định và chọn CN01 | Chỉ hai dòng CN01, tổng 300.000đ | Khớp; dropdown chỉ có CN01 và lựa chọn tất cả trong phạm vi được phép | Pass |
| Admin CN01 lọc riêng ngày 02/11 | 1 đơn 200.000đ; loại đơn hủy 999.000đ | UI/API/SQL đều 200.000đ | Pass |
| Admin CN01 gọi branchId=2 trực tiếp | HTTP 403 | 403, không trả số liệu CN02 | Pass |
| Employee gọi API doanh thu / không đăng nhập | 403 / 401 | 403 / 401 | Pass |
| Owner thử PUT chi nhánh | 403, không ghi dữ liệu | 403 | Pass |
| Ngày trống 01/01/2001 | API [], UI tổng 0 và thông báo trống | Đúng trên API và UI | Pass |
| Từ 02/11 đến 01/11 | 400; UI báo khoảng ngày hợp lệ, không hiện tổng 0/bảng trống | API 400; UI hiển thị lỗi, tổng/bảng bị ẩn | Pass |
| Chọn lại khoảng đúng sau lỗi | Lỗi được xóa, dữ liệu trở lại | CN02 300.000đ | Pass |
| Loading thật | Khóa lọc; hiện Đang xử lý; ẩn tổng/bảng | Đạt khi trì hoãn read SQL Test 20 giây bằng transaction rồi rollback, không ghi dữ liệu | Pass |
| Build API và Web | Thành công | 0 warning/error | Pass |
| Unit MvpRevenueValidationTests | 5 case hợp lệ/biên ngày/khoảng không hợp lệ | 5 Pass, 0 Fail, 0 Skip | Pass |
| Review Khang, QA Thuận | Người được giao review/retest | Chờ bàn giao | NotRun |
| Smoke public | URL/build deploy thật | Chưa có trong lượt này | NotRun |

HTTP/SQL verifier đạt **25 assertion** ngày 10/11 và chạy lại đạt **25 assertion** ngày 11/11 sau chỉnh creator fixture và tích hợp main mới; logout các phiên do verifier tạo. UI Owner/Admin được kiểm ngày 10/11 trên bản component cuối sau rebuild/restart; ảnh cuối đã ghi đè ảnh trung gian. Main mới chỉ thêm chặn khóa Admin đang được phân công trong SaveUser và tài liệu/tools, không đổi component, API doanh thu hoặc truy vấn doanh thu. Vì vậy ảnh UI giữ provenance main ae8c7f2 + blob component cuối; build/unit/HTTP retest ngày 11/11 có provenance main dbfa03d. Không ghi ảnh cũ thành ảnh của build mới. Đã đăng xuất các phiên UI cuối. Trong lúc chuẩn bị, một lượt verifier bị lỗi gom mảng PowerShell; em sửa cách enumerate và chạy lại đạt, không xem đó là lỗi sản phẩm hoặc lượt Pass.

Ngày trong bảng là dd/MM/yyyy; giá trị gửi API là ISO yyyy-MM-dd. Ô date native trên máy chụp dùng locale Chrome/Windows dạng MM/DD/YYYY, nên ảnh ô lọc 11/01/2026 nghĩa là ngày 01/11; không lấy hình thức hiển thị native làm bằng chứng đã hoàn tất mọi việc định dạng của WBS 3.6.2.4.

Ảnh thật, không dựng, không token/secrets: `docs/evidence/WBS-3.6.2.3/` gồm Owner tất cả/CN02/trống/lỗi và Admin phạm vi CN01/ngày 02/loading. Log thật và script ở `tests/sprint5/WBS-3.6.2.3/`. Không dùng build pass thay nghiệm thu nghiệp vụ.

## Cách chạy lại

Từ root nhánh này, với demo account/branch có sẵn từ core:

```powershell
sqlcmd -S localhost -E -C -b -d BranchFlow_Test -f 65001 -W -i tests/sprint5/WBS-3.6.2.3/Seed-RevenueBoundary.sql
sqlcmd -S localhost -E -C -b -d BranchFlow_Test -f 65001 -W -i tests/sprint5/WBS-3.6.2.3/Seed-RevenueBoundary.sql
dotnet build src/BranchFlow.Api -c Release
dotnet build src/BranchFlow.Web -c Release
dotnet test tests/BranchFlow.Tests -c Release --filter FullyQualifiedName~MvpRevenueValidationTests
# Khởi động API trên DB Test và Web trỏ đúng API; cấp mật khẩu hiện có qua process env, không ghi file:
./tests/sprint5/WBS-3.6.2.3/Verify-Revenue.ps1 -BaseUrl https://localhost:17317
```

Verifier so từng dòng API với SQL và kiểm bộ expected tường minh của fixture. Nếu máy khác có đơn khác cùng ngày/branch khiến tổng khác, cần ghi baseline/phân tích dữ liệu, không xóa đơn cũ để ép Pass. Không chạy baseline hoặc reset DB. Fixture header chỉ dành cho Test; không chạy script này trên Dev/public.

Để kiểm UI: Owner → Tổng quan → chọn 01–02/11/2026 → đối chiếu ba dòng → CN02 → ngày trống → khoảng ngược → khôi phục. Đăng xuất, đăng nhập Admin CN01 → cùng khoảng → kiểm chỉ CN01 → riêng 02/11. Test cross-branch thực hiện bằng verifier HTTP, không tạo option giả trong UI.

## Bàn giao và phần tiếp theo

Em bàn giao Khang code/commit và bộ minh chứng để review; Thuận nhận script/expected/ảnh để QA retest, đối chiếu WBS 3.6.2.5 và 3.1.2.5. Trường Tài nhận kết quả đối chiếu API đầu vào 3.6.1.3; lượt này không sửa backend. Thành dùng lại cho 3.6.2.4 và 4.6.2; Phước Thuần dùng đầu ra đã review cho 5.5.1. Đây là danh sách người nhận trong handoff/Jira, không giả đã được họ xác nhận hoặc đã gửi riêng.

Chờ review/QA và public; không tự đánh dấu Done cả SCRUM-126/Story SCRUM-105, không nhận hoàn thành các WBS tiếp theo, không copy estimate 2h thành giờ actual.
