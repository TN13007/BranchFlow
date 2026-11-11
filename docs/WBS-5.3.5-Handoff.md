# WBS-5.3.5 — Thiết kế slide demo và phân vai thuyết trình

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, kiểm tra gói slide/kịch bản và bàn giao **11/11/2026**. Ngày 22/11 trong lịch là kế hoạch. Jira tham chiếu: **SCRUM-107** (Subtask FE liên quan: **SCRUM-127**). Nhánh riêng: `docs/SCRUM-107-wbs-5.3.5-demo-slides` tách từ `origin/main` (**ae595fd**).

## Phần tiếp nhận và phần em đóng gói

Em tiếp nhận nguyên vẹn hai đầu ra của `WBS 5.3.5` từ working tree `D:\QLDA\BranchFlow\docs\`:
1. **`docs/Sprint5-BranchFlow-Demo.pptx`** (8 trang PowerPoint, `169.055 bytes`, SHA256 `5D0FD31A6C79FF85F081CDB2676932BEC12528F050BF838EB3F93393D513831A`, Git blob `447d32f66191053aaebf05b79161c0c4feeeecf6`): sử dụng ảnh chụp màn hình thật từ các lượt chạy local `BranchFlow_Test` ngày `10/11/2026`, không dùng ảnh dựng hoặc dữ liệu giả.
2. **`docs/Sprint5-Demo-Presentation.md`** (SHA256 `D54780EE17BA3B9058C85A458E0EB59948A64DAB3EBA99A134E053E2940CA35C`, Git blob `8d34b1f4e9dfeab645f3922cc3dbb89e5eb3beb1`): kịch bản trình bày 8 phút tương ứng 8 trang slide, phân vai đề xuất cho 5 thành viên và hướng dẫn chuẩn bị dữ liệu/xử lý sự cố trước buổi demo.

## Cấu trúc 8 slide & phân vai đề xuất (8 phút)

| Thời lượng | Slide | Nội dung chính | Người trình bày (đề xuất) |
|---|---|---|---|
| `0:00–1:00` | Slide 1–2 | Phạm vi MVP BranchFlow, kiến trúc 3 chi nhánh & ma trận quyền 4 vai trò (giữ phần mở rộng ở mục hoãn) | **Bá Thành Chí Khang** (PM) |
| `1:00–3:00` | Slide 3 | Luồng **Employee A** tạo đơn thực tế (`Trà đào cam sả` size M, 2 ly × `49.000đ` = `98.000đ`, kiểm tra giỏ và tạo đơn đúng 1 lần) | **Nguyễn Công Thành** (FE) |
| `3:00–4:30` | Slide 4 | Luồng **Admin CN01** điều chỉnh số ly (`2 → 3 ly`, `147.000đ` theo giá snapshot) có lý do & lịch sử audit, sau đó hủy đơn qua bước xác nhận và kiểm tra lại bên Employee | **Nguyễn Công Thành** (FE) |
| `4:30–5:45` | Slide 5 | Luồng **Quản lý kho thủ công** (nhập/xuất nguyên liệu thử riêng, chặn xuất vượt tồn `400`, chặn `RowVersion` cũ `409`, bán hàng không tự trừ kho) | **Nguyễn Phước Thuần / Nguyễn Anh Trường Tài** (BE) |
| `5:45–6:30` | Slide 6 | **Doanh thu theo ngày (giờ VN) và chi nhánh** (chỉ tính đơn `COMPLETED`, loại đơn `CANCELLED`; giải thích số liệu fixture tĩnh `128.000đ` và tổng DB Test sau các lượt test) | **Nguyễn Công Thành** (FE) |
| `6:30–7:15` | Slide 7 | **Kết quả kiểm thử** (phân biệt rõ tự kiểm chứng dev, regression tự động, QA độc lập và các ca còn `NotRun`/chờ xử lý) | **Nguyễn Đặng Hòa Thuận** (QA) |
| `7:15–8:00` | Slide 8 | **Tổng kết & điều kiện nghiệm thu còn lại** (Owner đọc hồ sơ, Android thật, smoke public HTTPS và rà soát nộp cuối) | **Bá Thành Chí Khang** (PM) |

## Case / expected / actual

| Ca kiểm chứng | Expected | Actual / Nguồn | Kết quả |
|---|---|---|---|
| Khớp số liệu trong slide và kịch bản với bộ dữ liệu demo `BF-WBS5-v1` và hành trình UAT | Đơn giá `49.000đ/ly` (`2 ly = 98.000đ`, `3 ly = 147.000đ`), tồn kho mẫu `7/15/4`, doanh thu fixture tĩnh `128.000đ`, giải thích rõ tổng DB Test sau retest (`128.020,50đ` trước retest / `215.020,50đ` sau 3 đơn retest) | Đã ghi chú rõ trong `docs/Sprint5-Demo-Presentation.md` và speaker notes của `docs/Sprint5-BranchFlow-Demo.pptx` | **Pass** |
| Ảnh minh họa trong slide là ảnh chạy thật, không lộ mật khẩu/token | Chỉ dùng ảnh từ `docs/evidence/sprint5-uat/`, rõ chữ, không chứa secrets | Đã kiểm tra 8 slide trong `Sprint5-BranchFlow-Demo.pptx` | **Pass** |
| Không mạo nhận biên bản họp hoặc xác nhận phân vai của các thành viên khác | Ghi rõ bảng phân vai là **đề xuất phục vụ tổng duyệt**, chờ cả nhóm 5 thành viên xác nhận | Đã ghi rõ tại dòng 21 của `docs/Sprint5-Demo-Presentation.md` | **Pass** |
| Tổng duyệt nhóm 5 thành viên & kiểm tra mở trực tiếp trong Microsoft PowerPoint trước giờ thuyết trình | Cả nhóm thống nhất phân vai và chạy thử slide + live demo trên URL chính thức | Chờ PM **Khang** tổ chức tổng duyệt nhóm | **NotRun / Chờ tổng duyệt** |

## Chưa đạt / phụ thuộc / người nhận

- **Người nhận tiếp theo**: Đầu vào cho **`WBS 5.3.6` (*Chuẩn hóa định dạng, link và kiểm cuối trước nộp*)** và gửi PM **Khang** cùng cả nhóm để chốt phân vai thuyết trình và tổng duyệt.
- **Lưu ý**: File `.pptx` được sinh tự động và kiểm tra qua bản dựng từng trang; trước buổi trình bày chính thức cần mở trực tiếp trên máy thuyết trình bằng Microsoft PowerPoint để rà lần cuối font chữ và tỷ lệ khung hình máy chiếu.
