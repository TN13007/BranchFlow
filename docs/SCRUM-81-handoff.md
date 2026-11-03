# SCRUM-81 — Form đăng nhập và layout

> Đây là bản ghi kiểm tra ở bước SCRUM-81 ban đầu. Phần auth/API client đã được bổ sung sau đó; xem [bàn giao FE Sprint 4 hiện tại](Sprint4-FE-handoff.md) để biết trạng thái mới nhất. Ảnh trong file này ghi nhận phiên bản UI ban đầu.

Ngày kiểm tra: 03/11/2026. Nhánh: `thanhnc`, tạo từ `main` tại `f49e391` (đã chứa baseline `2182dc4`).

## Kết quả

- Thêm `/login` bằng Blazor Interactive Server, Bootstrap có sẵn và CSS isolation.
- AuthLayout có header thương hiệu, vùng nội dung và footer; desktop hai cột, mobile chỉ giữ form.
- Form có nhãn, autocomplete, validation bắt buộc, tên đăng nhập tối đa 100 ký tự theo schema, nút hiện/ẩn mật khẩu dùng bàn phím được và vùng thông báo lỗi.
- Chỉ trim username. Chưa có API auth nên submit hợp lệ hiển thị thông báo chưa thể đăng nhập, xóa mật khẩu khỏi state và không tạo cookie/phiên hoặc chuyển sang màn đã đăng nhập.
- Trang chủ có liên kết vào form. Không đổi Program, Routes, backend hoặc DB.

## Đối chiếu Figma

Đã đọc danh sách trang và frame từ [Figma BranchFlow](https://www.figma.com/design/TluV1WGVSLMddD51pXzIlv/BranchFlow?node-id=10-504). File hiện có ba trang Chủ sở hữu / Quản trị / Nhân viên; chưa tìm thấy frame login. Đã đọc design context và ảnh màn [Nhân viên — Trang chủ](https://www.figma.com/design/TluV1WGVSLMddD51pXzIlv/BranchFlow?node-id=10-522) để tham khảo nhận diện.

Form login là bố cục đề xuất cần Khang review, chưa có cơ sở kết luận khớp một frame login đã duyệt. Dùng màu brand `#08796b`, ink `#17252a`, line `#dfe3df`, nền sáng và nhãn BF từ thiết kế. Nội dung giới thiệu, bố cục hai cột, bo góc và mobile được đề xuất cho form này. Font CSS ưu tiên Noto Sans theo thiết kế; máy không có font sẽ dùng Segoe UI/sans-serif, chưa đóng gói font. Không sử dụng ảnh chụp thiết kế làm asset UI.

## Kiểm tra thực tế

Build solution Release bằng SDK 10.0.300: thành công, 0 warning / 0 error.

```powershell
& "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe" build BranchFlow.slnx -c Release --no-restore
& "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe" run --project src/BranchFlow.Web -c Release --no-build --launch-profile https
```

Mở `https://localhost:7132/login`. Host báo chứng chỉ dev chưa được tin cậy; kiểm tra browser headless đã bỏ qua lỗi chứng chỉ trong môi trường local, chưa thay trust store của máy.

Kiểm tra trình duyệt Edge headless bằng Playwright cài tạm ngoài repo, không thêm dependency cho ứng dụng:

| Case | Expected | Actual |
|---|---|---|
| Bấm Hiện rồi Ẩn | Đổi đúng type input | Pass |
| Submit trống | Lỗi bắt buộc ở cả hai trường | Pass |
| Username 101 ký tự | Báo tối đa 100 ký tự | Pass |
| Username chỉ khoảng trắng | Báo bắt buộc nhập | Pass |
| Submit dữ liệu hợp lệ cho bước UI | Trim username, báo chưa thể đăng nhập, xóa mật khẩu | Pass |
| API request / cookie auth | Chưa gọi auth, chưa tạo phiên giả | Pass |
| Mobile 360 × 800 có thông báo lỗi | Không tràn ngang, thấy hai trường | Pass |
| Lỗi runtime browser | Không phát sinh | Pass |

Đã xem ảnh render desktop và mobile. Ảnh kiểm tra dùng tên đăng nhập minh họa, không có mật khẩu/token:

- [Desktop 1440 × 1000](evidence/scrum-81/login-desktop.png)
- [Mobile 360 × 800](evidence/scrum-81/login-mobile.png)
- [Mobile khi submit chưa có auth](evidence/scrum-81/login-mobile-error.png)

## Phụ thuộc và giới hạn

SCRUM-83 sẽ bổ sung HTTP login/logout handler với antiforgery, cookie và store token phía server, redirect full reload và loading theo request thật. Chưa có delay/request giả để trình diễn loading. Chưa kiểm thử tài khoản đúng/sai, refresh, logout, hai user hoặc API/SQL; đây là Not Run do auth chưa triển khai.

Chưa có frame login để nghiệm thu độ khớp thiết kế. Chưa chuyển Jira Done, chưa tạo PR hoặc push. Khang cần review bố cục đề xuất và QA xác nhận trên commit bàn giao.

## Comment Jira đề xuất

SCRUM-81: Đã dựng form /login và AuthLayout trên Blazor Interactive Server + Bootstrap, có validation và responsive 360px. Build Release SDK 10.0.300 đạt 0 warning/error; các case UI trong bảng trên đã chạy bằng Edge headless. Ảnh đính kèm tại docs/evidence/scrum-81. Figma chưa có frame login, giao diện đề xuất theo nhận diện màn Nhân viên cần Khang review. Auth/loading theo API thật chưa triển khai, thuộc SCRUM-83; chưa coi login API là Pass. Commit/PR: bổ sung khi bàn giao. Trạng thái: chờ review, chưa đủ cơ sở Done.
