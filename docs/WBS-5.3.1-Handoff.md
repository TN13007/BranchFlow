# Bàn giao WBS 5.3.1 — Tổng hợp 9 hồ sơ Bài tập Tuần 2 và minh chứng

Em là Công Thành, có công cụ hỗ trợ, tổng hợp và bàn giao danh mục đối chiếu 9 hồ sơ Bài tập Tuần 2 cho **WBS 5.3.1** (Story tham chiếu `SCRUM-107`).

## 1. Thông tin chung
- **WBS**: `5.3.1` — Tổng hợp 9 hồ sơ Bài tập Tuần 2 và minh chứng (`BF-NB-5.3.1`)
- **Người thực hiện**: Nguyễn Công Thành (FE & hỗ trợ tài liệu)
- **Jira tham chiếu**: `SCRUM-107` (`https://khangba153.atlassian.net/browse/SCRUM-107`)
- **Lịch kế hoạch ProjectLibre**: `2026-11-23 14:00 -> 2026-11-23 16:00` (2h) — Hạn nội bộ mới: **Thứ Sáu, 13/11/2026 23:00**
- **Ngày soạn thảo bản gốc**: `10/11/2026` (trên working tree `thanhnc`, nền `47541ee`)
- **Ngày kiểm chứng định dạng & đóng gói bàn giao**: `11/11/2026` (trên nhánh sạch `docs/SCRUM-107-wbs-5.3.1-week2-dossier` tách từ `origin/main` `ae595fd3872167a70aa9e3f537601682c31e2e28`)
- **Người nhận bàn giao**: PM Bá Thành Chí Khang (đối chiếu WBS, xác minh quyền mở 9 link Google Docs/Sheets/FigJam bằng tài khoản nhóm và tổng hợp hồ sơ nộp cuối)

---

## 2. Đầu ra đã thực hiện
1. **Tài liệu danh mục đối chiếu 9 hồ sơ Tuần 2 (`docs/Sprint5-Week2-Dossier-Index.md`)**:
   - Giữ nguyên bản gốc soạn ngày `10/11/2026` (SHA256 `B206963AFED33C19C45675B0F76A3FCA6843E63FDEC47E9EE5DC2E7EAA681193`, Git blob `bac5ed52b842ec24415c38f2e26adc776539500b`).
   - Tổng hợp đầy đủ 09 hồ sơ gốc trích từ `Nhom6_BranchFlow_DanhMucNopTuan2.docx`:
     1. **01 — Stakeholder Register**: 10 bên liên quan, Power/Interest Grid (`Google Sheets`).
     2. **02 — Requirement List**: 39 FR, 8 NFR, Actor, độ ưu tiên và AC (`Google Sheets`).
     3. **03 — Use Case Diagram**: UML tổng quan (4 Actor, 7 phân hệ) và 4 đặc tả Use Case (`FigJam node 7-259`).
     4. **04 — BPMN Diagram**: Quy trình tạo đơn, báo đơn sai, AS-IS/TO-BE (`FigJam node 9-425`).
     5. **05 — Scope Statement**: Mục 4.1–4.8 (`Google Docs`).
     6. **06 — MVP Definition**: 7 phân hệ gốc Tuần 2, MoSCoW (`Google Docs`).
     7. **07 — Work Breakdown Structure (WBS)**: 5 nhóm, 245 task gốc (`Google Sheets`).
     8. **08 — Product Backlog**: 47 item (`Google Sheets`).
     9. **09 — Requirements Traceability Matrix (RTM)**: 47 dòng truy vết (`Google Sheets`).
   - **Tách bạch lịch sử Tuần 2 và phạm vi MVP hiện hành**: Ghi rõ các con số `39 FR/8 NFR`, `47 item`, `245 hoạt động`, `7 phân hệ` là mô tả lịch sử Tuần 2; không dùng để khẳng định các phần đã hoãn sau 26/11 (bảng lương, điều chuyển, kiểm kê, báo sai nhiều bước) đã triển khai trong MVP hiện hành.

---

## 3. Kết quả tự kiểm chứng (`11/11/2026` vs `10/11/2026`)

| Hạng mục kiểm chứng | Ngày thực hiện | Kết quả kỳ vọng (Expected) | Kết quả thực tế (Actual) | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| Đối chiếu 09 hồ sơ và URL từ danh mục gốc Tuần 2 | `10/11/2026` & `11/11/2026` | Đủ 09 mục (01–09) kèm mô tả nội dung và hyperlink gốc từ `Nhom6_BranchFlow_DanhMucNopTuan2.docx` | Đủ 09 mục và 09 URL Google Docs/Sheets/FigJam trong bảng | **Pass** |
| Tách bạch phạm vi lịch sử Tuần 2 và MVP hiện hành | `10/11/2026` & `11/11/2026` | Không tạo lại bằng chứng quá khứ; ghi rõ phần hoãn sau 26/11 theo `mvp-scope.md` | Có mục *Đối chiếu phạm vi và việc còn lại* nêu rõ sự khác biệt | **Pass** |
| Xác minh quyền mở 09 link Google/FigJam bằng tài khoản nhóm | `11/11/2026` | Chủ nguồn / PM kiểm tra quyền xem bằng tài khoản nhóm và đối chiếu đúng sheet/version | Chưa đăng nhập tài khoản nhóm để kiểm quyền từng link online (`ONLINE NOT VERIFIED`) | **NotRun** |

---

## 4. Tồn đọng & Phụ thuộc bàn giao
1. **Xác minh quyền truy cập 09 URL online (`NotRun`)**: PM Khang / chủ sở hữu file Google Docs, Google Sheets và FigJam cần kiểm tra quyền mở bằng tài khoản nhóm trước khi đóng gói nộp chính thức.
2. **Trạng thái task**: Hoàn tất danh mục đối chiếu nội bộ và bàn giao cho PM Khang (`SCRUM-107`), **chưa tự đánh dấu Done**.
