# Bàn giao Frontend Sprint 4 — Thành

Ngày cập nhật: 03/11/2026. Nhánh làm việc: `thanhnc`, tạo từ `main` tại `f49e391`, đã có baseline `2182dc4`. Đã fetch và kiểm tra `origin/khang`; hiện vẫn là baseline, chưa có controller nghiệp vụ. Thành viên xác nhận chưa có API bàn giao ngoài repo. Chưa push, tạo PR hoặc đổi Jira.

## Phạm vi và trạng thái

| Issue | Đã triển khai phía FE | Phần còn cần để nghiệm thu |
|---|---|---|
| SCRUM-81 | `/login`, AuthLayout, validation, hiện/ẩn mật khẩu, responsive | Khang review bố cục login vì Figma chưa có frame login |
| SCRUM-82 | Menu, tìm tên/danh mục, drawer chọn size/topping/ghi chú, thêm/sửa/xóa dòng, tổng tạm tính, payload theo contract | Review UI và minh chứng trên commit bàn giao |
| SCRUM-83 | HTTP handler login/logout có antiforgery, cookie, store phiên server, refresh có khóa, guard route | API Thuần SCRUM-71–73, seed tài khoản và kiểm thử phiên API thật |
| SCRUM-84 | Client me/accessible/menu, chọn branch từ API, trạng thái loading/rỗng/lỗi, giữ giỏ và kiểm lại menu | Endpoint/menu seed của Tài SCRUM-76, tài khoản/assignment của Thuần |
| SCRUM-85 | POST create, giữ giỏ khi lỗi, chặn gửi lặp, kết quả server, mine/detail, xử lý kết quả chưa rõ | API SCRUM-78/79, đối chiếu mã/số tiền với SQL thật |
| SCRUM-86 | Rà soát/sửa binding nút, số ly, hình món, focus dialog, responsive; bộ kiểm tra và hướng dẫn chạy | QA/review thực tế sau khi BE bàn giao, nghiệm thu luồng xuyên suốt |

Các task tích hợp đã có mã FE, chưa đủ AC để Done. Không dùng dữ liệu mẫu hoặc API fixture làm minh chứng API/SQL thật.

## Cách chạy

Từ root repo:

```powershell
dotnet restore BranchFlow.slnx
dotnet build BranchFlow.slnx -c Release --no-restore
dotnet run --project src/BranchFlow.Web -c Release --no-build --launch-profile https
```

Cập nhật cách chạy: global.json dùng latestFeature, cho phép SDK .NET 10.0 từ 10.0.300 trở lên. Đã build 0 warning/error và chạy lại 21 test thành công bằng SDK hệ thống 10.0.401. Không còn cần alias hoặc đường dẫn SDK người dùng. Nếu terminal còn alias dotnet cũ, xóa bằng Remove-Item Alias:dotnet -ErrorAction SilentlyContinue. API mặc định `https://localhost:7107/`, Web `https://localhost:7132/`. Chứng chỉ dev của máy hiện chưa được trust; kiểm tra HTTPS cần chứng chỉ local được tin cậy. Không có đoạn tắt kiểm tra chứng chỉ trong HttpClient ứng dụng.

Xem thử UI không cần API:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project src/BranchFlow.Web -c Release --no-build --no-launch-profile --urls http://localhost:5133
```

Mở `http://localhost:5133/preview/orders`. Trang chỉ bật nội dung trong Development, có banner DỮ LIỆU MẪU và không gọi API/ghi order. Nhánh UI mẫu sử dụng `PreviewMenu`, không có auth giả hay tài khoản mặc định. HTTP host này chỉ dùng xem thử; login handler yêu cầu HTTPS.

## Các route

- `/login`: form và thông báo auth, POST HTTP qua `/auth/login`; thay circuit sau redirect.
- `/account`: tài khoản đã đăng nhập; role ngoài EMPLOYEE có thông báo phạm vi Sprint.
- `/orders/create`: EMPLOYEE, menu/giỏ/xác nhận.
- `/orders/mine`: EMPLOYEE, tìm mã và lọc businessDate tùy chọn.
- `/orders/{id}`: EMPLOYEE, snapshot đơn và kết quả vừa tạo từ API.
- `/auth/logout`: chỉ POST, antiforgery, thu hồi API rồi xóa cookie/store. Khi API lỗi vẫn xóa local và thông báo chưa xác nhận thu hồi từ API.

## Bàn giao API cho Thuần/Tài

DTO network đề xuất đặt tại `src/BranchFlow.Application/DTOs/Sprint4/Contracts.cs`, theo contract có trong hai gói ZIP. Đây là DTO để FE biên dịch và gọi mạng; BE cần đối chiếu/review trước tái sử dụng, chưa tuyên bố đã chốt đồng bộ với code BE. Không chỉnh Models, DbContext, SQL hoặc repository backend.

API client dùng chín endpoint trong contract: login, refresh, logout, me, branches/accessible, order-menu, orders POST, orders/mine và orders/{id}. Thành công là DTO trực tiếp. Lỗi đọc ProblemDetails validation có giới hạn độ dài; 500 hiển thị thông báo chung. 401 cho GET được refresh rồi thử lại tối đa một lần; 403 không refresh. POST tạo đơn chủ động gia hạn trước khi gửi, không tự replay dù timeout hoặc 401.

Cấu hình Web:

```json
{"Api":{"BaseUrl":"https://localhost:7107/"}}
```

Có thể ghi đè bằng `Api__BaseUrl` khi chạy môi trường riêng. Named HttpClient không tự redirect, không dùng cookie container, không ghi log request auth. Không cần CORS cho lời gọi server → API. Token không nằm trong browser storage, cookie ticket hoặc HTML.

Login lấy user/role qua `/api/auth/me`. Store RAM liên kết từng cặp token với khóa ngẫu nhiên 256 bit, cookie chỉ có định danh/role/khóa. Cookie `__Host-BranchFlow.Auth`: Secure, HttpOnly, SameSite=Lax, không sliding, không sống quá refresh expiry và giới hạn phiên Web 7 ngày. Refresh/logout dùng cùng khóa phiên; lỗi rotation không rõ kết quả buộc đăng nhập lại, tránh dùng lại refresh token cũ. Circuit kiểm tra store mỗi 15 giây, còn API kiểm tra phiên/quyền thật ở mỗi request nghiệp vụ.

Giỏ giữ `ProductSizeId`, quantity nguyên dương, topping ID và quantity 1–3 từng loại, mỗi noteGroup tối đa một option; không có ghi chú tự do. Hai cấu hình khác nhau giữ thành hai dòng. Trước xác nhận tải lại menu: món bị tắt báo sửa giỏ; giá đổi cập nhật preview và yêu cầu xác nhận lại. API vẫn là nơi quyết định giá/quyền cuối cùng.

Payload chỉ có:

```json
{"branchId":1,"items":[{"productSizeId":101,"quantity":2,"toppings":[{"toppingId":5,"quantity":3},{"toppingId":6,"quantity":1}],"noteOptionIds":[7]}]}
```

ID trên chỉ dùng fixture; luồng thật lấy từ menu. Tổng mẫu 98.000đ chỉ là preview, không gửi lên API. Chỉ HTTP 201 và response hợp lệ mới xóa giỏ, sau đó GET detail theo ID. Nếu timeout/mất mạng/500 sau POST, giữ giỏ, khóa chỉnh/gửi tiếp và hướng người dùng vào mine; chỉ mở lại sau người dùng xác nhận đã kiểm tra chưa có đơn. Đây không phải idempotency backend.

## Kiểm tra đã thực hiện

- Build toàn solution Release bằng SDK 10.0.300: thành công, 0 warning / 0 error.
- Kết quả cuối: 21 test xUnit qua, 0 fail/skip; 9 kiểm tra HTTP với contract fixture qua.
- xUnit: cart validation, snapshot request không giá, thay đổi giá/menu, kết quả chưa rõ, tách hai session, refresh đồng thời, 401/403/500, POST không replay, chỉ chấp nhận create 201, tiền có phần lẻ và giờ Việt Nam.
- HTTP contract fixture: antiforgery, credential lỗi, 429, trim username và giữ khoảng trắng password, cookie flags, hai cookie jar, F5 tương đương GET mới, role ngoài EMPLOYEE, logout và lỗi logout. Fixture nằm trong `tests/contract`, chỉ phục vụ test, bind loopback và tự dừng Web test host/fixture khi xong.
- Browser trên `/preview/orders`: mở drawer, chặn thiếu size, topping chặn ở 3, hai loại topping + 2 ly = 98.000đ; chỉnh dòng, giảm topping về 0 → 68.000đ, chặn số ly 1.5, không tràn ngang ở 360px. Đã lọc danh mục, tìm kiếm không có kết quả, thêm cùng món size M và L thành hai dòng riêng (tổng 68.000đ), xóa dòng M còn dòng L 38.000đ. Nút tạo đơn bị khóa rõ trong preview.

Chạy lại:

```powershell
dotnet test BranchFlow.slnx -c Release --no-restore
node tests/contract/verify-web.mjs
```

HTTP fixture dùng các port riêng 19107 và 7134, không thay cấu hình Web mặc định hoặc DB. HTTPS test client bỏ qua chứng chỉ dev chỉ bên trong script kiểm tra local; HttpClient ứng dụng vẫn kiểm chứng chỉ bình thường. Không dùng mật khẩu/tài khoản thật trong fixture.

## Đối chiếu thiết kế và giới hạn

Đã đọc design context/screenshot các frame Nhân viên: `10:523` menu, `13:14090` configurator, `10:525` cart, `10:526` mine, `27:2548` thành công. [File Figma](https://www.figma.com/design/TluV1WGVSLMddD51pXzIlv/BranchFlow?node-id=10-523).

Asset tải vào `wwwroot/images`: đúng ảnh trà đào/cà phê/matcha/trà vải/bạc xỉu cho fixture, icon coffee/house/trash giữ kích thước SVG gốc. Luồng thật lấy ảnh từ API, không dùng ảnh mẫu thay ảnh sản phẩm. Không còn URL asset tạm Figma trong source.

Khác biệt có chủ đích: bỏ role preview bar/nút module ngoài Sprint; thêm link mine và logout thật; không hiển thị giả lịch sử giao dịch hoặc số liệu dashboard. Không lấy ngoại lệ topping Up size=1 hoặc trạng thái ADJUSTED từ prototype. Menu contract chỉ có món còn bán nên không dựng món tắt giả trong luồng thật. Noto Sans chưa đóng gói; CSS có fallback Segoe UI/sans-serif. Login chưa có frame riêng. Cần review visual cuối với Khang.

Giới hạn RAM demo: restart Web mất phiên; F5/tab mới giữ auth nếu store còn nhưng tạo circuit mới, giỏ hiện không được lưu bền qua F5. Không triển khai quản lý thiết bị, module kho/lương/admin, idempotency key hoặc backend endpoint/seed.

## Checklist khi API được bàn giao

1. Thuần cung cấp URL/cấu hình HTTPS, role/user/assignment seed qua kênh riêng; Tài cung cấp menu/giá/ID thực và bộ request mẫu.
2. So sánh DTO/JSON và lỗi 400/401/403/404/409/429; xác nhận rotation/logout đúng contract. Không đổi payload hoặc quyền âm thầm để né khác biệt.
3. Chạy login đúng/sai/khóa, hết hạn, hai browser/user, tab mới/F5, logout rồi request bằng access cũ, restart Web.
4. Chạy branch A/B, employee chưa phân công, menu rỗng/tắt món/topping, đổi giá trước confirm.
5. Tạo đơn thật, đọc mine/detail, đối chiếu SQL cùng code/total/businessDate/snapshot. Thử lỗi validation/500/mạng và double click; lưu expected/actual đã che token.
6. QA retest đúng commit, Khang review PR hướng `khang`; đủ AC/minh chứng mới Done.

## Comment Jira đề xuất

Đã triển khai phần FE cho SCRUM-81–86 trên nhánh thanhnc; xem route, file và kết quả kiểm tra tại docs/Sprint4-FE-handoff.md. Unit/HTTP fixture chứng minh logic FE và Web handler, không thay nghiệm thu API/SQL thật. Backend chưa bàn giao (đã xác nhận với thành viên), SCRUM-83–85 chờ đầu vào Thuần/Tài; SCRUM-86 chờ QA sau tích hợp. Khang cần review login đề xuất và khác biệt prototype. Chưa chuyển Done. Bổ sung commit/PR và minh chứng thật khi bàn giao.

Tham khảo kỹ thuật: [Blazor security](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10.0), [antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0).
