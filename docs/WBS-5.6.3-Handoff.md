# WBS-5.6.3 — Soạn Mini-PM Report – Công Thành

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, cập nhật truy vết commit/PR và bàn giao **11/11/2026**. Ngày 21/11 trong lịch là kế hoạch. Jira tham chiếu: **SCRUM-107** (Subtask FE liên quan: **SCRUM-127**). Nhánh riêng: `docs/SCRUM-107-wbs-5.6.3-minipm-thanh` tách từ `origin/main` (**ae595fd**).

## Phần tiếp nhận và phần em đóng gói

Em tiếp nhận nguyên vẹn bản báo cáo cá nhân **`docs/Sprint5-MiniPM-Thanh.md`** đã soạn tại `D:\QLDA\BranchFlow\docs\Sprint5-MiniPM-Thanh.md` (trùng khớp byte-for-byte SHA256 `463CCEC0A9715C7614DB651716A7EB743DD5443D529390784F9F2EFC1266330E`, Git blob `77b4451c857687a961dd11e0f3e8b02701623f06`) và bổ sung danh mục đối chiếu đầy đủ các nhánh/commit/comment Jira mà em đã thực hiện và bàn giao đến ngày **11/11/2026**.

Tóm tắt đóng góp cá nhân của Công Thành trong Sprint 5 (20 hoạt động / 40 giờ kế hoạch):
1. **Nhóm Giao diện & Tích hợp API (Mục 1 — 7 WBS, 14 giờ kế hoạch)**:
   - `WBS 3.4.2.1` (`SCRUM-124`): Kiểm chứng và bàn giao luồng Admin điều chỉnh số ly / hủy đơn có lý do và audit (`feat/SCRUM-124-wbs-3.4.2.1-handoff`, commit `a556bcc` / `4d14af2` / `e2db2b5`).
   - `WBS 3.6.2.3` & `3.6.2.4` (`SCRUM-126`): Kiểm chứng lọc ngày/chi nhánh theo quyền và bổ sung trạng thái loading/empty/error doanh thu trong `ManagementWorkspace.razor` (`1488e9e`, `cf6ef7b`, đã merge vào `main`).
   - `WBS 3.5.1.5` (`SCRUM-125`): Bổ sung retry, cảnh báo khi POST kho chưa xác định kết quả, format số lượng `vi-VN` trong `ManagementStock.razor` và test tích hợp đồng thời (`1e2a678`, đã merge vào `main`).
   - `WBS 3.2.2.5` (`SCRUM-122`): Giới hạn nút/form sửa hồ sơ nhân viên theo vai trò trong `ManagementWorkspace.razor` (`10ba6e0`, Comment `10302`); ghi nhận rõ blocker backend `GET /api/management/users` trả `403` với Owner để chờ Phước Thuần (`3.2.2.4`) và PM Khang chốt.
   - `WBS 3.4.3.1` (`SCRUM-124`): Kiểm chứng và bàn giao trang lịch sử `/orders/mine` và chi tiết `/orders/{id}` của Employee (`d4d2696`, Comment `10303`).
   - `WBS 3.3.1.5` (`SCRUM-123`): Bổ sung retry, tách trạng thái loading/empty/error và format giá thập phân `vi-VN` trong `ManagementMenu.razor` (`7f53486`, Comment `10304`).
2. **Nhóm Dữ liệu Demo (Mục 2 — 3 WBS, 6 giờ kế hoạch)**:
   - `WBS 5.1.1`, `5.1.2`, `5.1.3` (`SCRUM-127`): Hoàn thiện và kiểm chứng idempotent bộ dữ liệu mẫu `BF-WBS5-v1` (3 chi nhánh, 4 vai trò, menu, 4 đơn theo trạng thái, tồn kho `7/15/4`, doanh thu fixture `128.000đ`) trên `BranchFlow_Dev` và `BranchFlow_Test` (`b7184c7`, `6f99364`, `095fdbf`, đều đã merge vào `main`).
3. **Nhóm UAT, Responsive & Tương thích trình duyệt (Mục 3 — 3 WBS, 6 giờ kế hoạch)**:
   - `WBS 4.4.1` (`SCRUM-131`): Xây dựng và chạy tự kiểm chứng 26 kịch bản UAT (`U01–U26`, bao gồm bổ sung retest `U25` gửi đơn 2 lần và proxy fault-inject `401`/timeout sau POST đơn) (`4b04b7f`).
   - `WBS 4.6.2` (`SCRUM-127`): Sửa lỗi tràn ô chọn phân công ở `768px` (`business.css`) và tràn khung menu Employee ở `360px` (`order-menu.css`), kèm 24 phép đo DOM và 28 ảnh chụp ở `360/768/1366px` (`fe9a0e4`, Comment `10300`).
   - `WBS 4.6.5` (`SCRUM-127`): Đánh giá tương thích Chrome và Edge desktop (Owner đọc + Admin Tổng ghi giá lẻ `15,75đ`), ghi nhận rõ Android thật và public HTTPS là `NotRun` (`96d8763`, Comment `10301`).
4. **Nhóm Hồ sơ, Báo cáo, User Guide & Slide thuyết trình (Mục 4 — 7 WBS, 14 giờ kế hoạch)**:
   - `WBS 5.3.3` (`131ca46`, Comment `10305`), `WBS 5.2.3` (`90456ca`), `WBS 5.6.3` (nhánh này) và các gói `5.3.5`, `5.3.1`, `5.6.6`, `5.3.6`.

## Case / expected / actual

| Ca kiểm chứng | Expected | Actual / Nguồn | Kết quả |
|---|---|---|---|
| Truy vết đầy đủ đóng góp cá nhân theo mã WBS, nhánh, commit và link minh chứng | Tách rõ phần core tiếp nhận và phần Công Thành thực hiện; mọi commit/link mở được | Khớp giữa `docs/Sprint5-MiniPM-Thanh.md` và `docs/WBS-5.6.3-Handoff.md` | **Pass** |
| Trung thực về giờ công (effort) và quyền ký nhận | Giữ nguyên 40h kế hoạch (20 WBS × 2h), không bịa giờ thực tế (actual) khi chưa có timesheet nhóm, không ký thay thành viên khác | Đã ghi rõ trong `docs/Sprint5-MiniPM-Thanh.md` mục *Tiến độ, rủi ro và bài học* | **Pass** |
| Bài học kinh nghiệm kỹ thuật rút ra từ thực tế kiểm chứng | Nêu bài học cụ thể từ lỗi thật đã gặp và sửa | (1) Kiểm tra `document.scrollWidth <= clientWidth` chưa đủ phát hiện phần tử con bị cắt (`overflow`), phải đo bounding box khối con; (2) Đối chiếu doanh thu phải khóa cùng thời điểm và cùng bộ dữ liệu; (3) Tự động hóa nhập số thập phân Blazor cần sự kiện phím/blur thật | **Pass** |

## Chưa đạt / phụ thuộc / người nhận

- **Người nhận tiếp theo**: Đầu vào trực tiếp cho **`WBS 5.6.6` (*Tổng hợp 5 báo cáo cá nhân*)** và gửi PM **Khang** review, xác nhận đóng góp cá nhân trong hồ sơ tổng kết Sprint 5.
- **Tồn đọng**: Để hoàn tất bảng gộp 5 người ở `WBS 5.6.6`, vẫn cần nhận đủ báo cáo cá nhân đã xác nhận của các thành viên còn lại (`5.6.1` Khang, `5.6.2` Phước Thuần, `5.6.4` Trường Tài và bản xác nhận cuối của `5.6.5` Hòa Thuận).
