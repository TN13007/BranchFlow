# Bàn giao WBS 5.3.6 — Rà soát định dạng, nguồn, đường dẫn và hạn nộp

Em là Công Thành, có công cụ hỗ trợ, hoàn thiện và bàn giao danh mục rà soát cuối Mục 4 (định dạng, nguồn, đường dẫn, hạn nộp) cho **WBS 5.3.6** (Story tham chiếu `SCRUM-107`).

## 1. Thông tin chung
- **WBS**: `5.3.6` — Rà soát định dạng, nguồn, đường dẫn và hạn nộp (`BF-NB-5.3.6`)
- **Người thực hiện**: Nguyễn Công Thành (FE & hỗ trợ tài liệu)
- **Jira tham chiếu**: `SCRUM-107` (`https://khangba153.atlassian.net/browse/SCRUM-107`)
- **Lịch kế hoạch ProjectLibre**: `2026-11-25 14:00 -> 2026-11-25 16:00` (2h) — Hạn nội bộ mới: **Thứ Sáu, 13/11/2026 23:00**
- **Ngày soạn thảo bản gốc**: `10/11/2026` (trên working tree `thanhnc`, nền `47541ee`)
- **Ngày kiểm chứng định dạng & đóng gói bàn giao**: `11/11/2026` (trên nhánh sạch `docs/SCRUM-107-wbs-5.3.6-final-review-checklist` tách từ `origin/main` `ae595fd3872167a70aa9e3f537601682c31e2e28`)
- **Tiền nhiệm theo lịch**: `5.3.4` (Khang — Tổng hợp kiểm thử, UAT và rủi ro), `5.3.5` (Công Thành — Slide demo & phân vai), `5.6.6` (Công Thành — Gộp 5 báo cáo cá nhân)
- **Người nhận bàn giao**: PM Bá Thành Chí Khang (`SCRUM-107`)

---

## 2. Đầu ra đã thực hiện
1. **Tài liệu danh mục và bảng rà soát Mục 4 (`docs/Sprint5-Section4-Index.md`)**:
   - Giữ nguyên bản gốc soạn ngày `10/11/2026` (SHA256 `83C2C69211B3603F9E7103984A6DD55439A47D200A73A7646D05AF6F9DF55049`, Git blob `b001532181b0bba92221047d706d7e89abc4def6`).
   - Tổng hợp trạng thái toàn bộ 7 đầu ra tài liệu Mục 4 của Công Thành (`5.3.3`, `5.2.3`, `5.6.3`, `5.3.5`, `5.3.1`, `5.6.6`, `5.3.6`) và bảng rà soát 10 tiêu chí (mã WBS, lịch cũ vs hạn mới `13/11`, phạm vi MVP, số liệu doanh thu, kết quả test, minh chứng ảnh thật, nguồn Tuần 2, link local, link online, trạng thái nộp/review).
2. **Log kiểm tra liên kết (`tests/sprint5/Section4-Link-Check.txt`)**:
   - Giữ nguyên log kiểm tra liên kết ngày `10/11/2026` (SHA256 `DB1C8D1B830C82D9C184B0900CF9000F8225D021DB46F8DEA028767C4920A5C1`, Git blob `780193d2a8842ccce1960e90a3e0797bb209569d`):
     - **25/25 liên kết nội bộ (`PASS`)** giữa 7 tài liệu Mục 4 (`missing local targets: 0`).
     - **09/09 liên kết ngoài (`ONLINE NOT VERIFIED`)** từ danh mục Tuần 2 được ghi rõ trạng thái chờ xác minh bằng tài khoản nhóm.
3. **Tổng kết truy vết toàn bộ 20/20 WBS của Công Thành (đã push lên GitHub & đóng gói ZIP trước hạn `13/11/2026`)**:
   - `WBS 5.1.1`: `feat/SCRUM-127-wbs-5.1.1-handoff` (`b7184c7`, đã merge `main`)
   - `WBS 3.4.2.1`: `feat/SCRUM-124-wbs-3.4.2.1-handoff` (`e2db2b5`)
   - `WBS 4.4.1`: `docs/SCRUM-131-wbs-4.4.1-thanh-uat` (`4b04b7f`)
   - `WBS 5.1.2`: `feat/SCRUM-127-wbs-5.1.2-demo-menu` (`6f99364`, đã merge `main`)
   - `WBS 3.6.2.3`: `feat/SCRUM-126-wbs-3.6.2.3-revenue` (`1488e9e`, đã merge `main`)
   - `WBS 3.6.2.4`: `docs/SCRUM-126-wbs-3.6.2.4-revenue-states` (`cf6ef7b`, đã merge `main`)
   - `WBS 5.1.3`: `docs/SCRUM-127-wbs-5.1.3-stock-revenue` (`095fdbf`, đã merge `main`)
   - `WBS 3.5.1.5`: `feat/SCRUM-125-wbs-3.5.1.5-stock-ui` (`1e2a678`, đã merge `main`)
   - `WBS 4.6.2`: `test/SCRUM-127-wbs-4.6.2-responsive` (`fe9a0e4`)
   - `WBS 4.6.5`: `docs/SCRUM-127-wbs-4.6.5-browser-compatibility` (`96d8763`)
   - `WBS 3.2.2.5`: `feat/SCRUM-122-wbs-3.2.2.5-employee-profiles` (`10ba6e0`)
   - `WBS 3.4.3.1`: `docs/SCRUM-124-wbs-3.4.3.1-employee-history` (`d4d2696`)
   - `WBS 3.3.1.5`: `feat/SCRUM-123-wbs-3.3.1.5-system-menu-ui` (`7f53486`)
   - `WBS 5.3.3`: `docs/SCRUM-107-wbs-5.3.3-wbs-gantt-sprint-report` (`131ca46`)
   - `WBS 5.2.3`: `docs/SCRUM-127-wbs-5.2.3-user-guide` (`90456ca`)
   - `WBS 5.6.3`: `docs/SCRUM-107-wbs-5.6.3-minipm-thanh` (`39de709`)
   - `WBS 5.3.5`: `docs/SCRUM-107-wbs-5.3.5-demo-slides` (`16ef3ca`)
   - `WBS 5.3.1`: `docs/SCRUM-107-wbs-5.3.1-week2-dossier` (`edb2b44`)
   - `WBS 5.6.6`: `docs/SCRUM-107-wbs-5.6.6-five-personal-reports` (`6adf332`)
   - `WBS 5.3.6`: `docs/SCRUM-107-wbs-5.3.6-final-review-checklist` (nhánh hiện tại)

---

## 3. Kết quả tự kiểm chứng (`11/11/2026` vs `10/11/2026`)

| Hạng mục kiểm chứng | Ngày thực hiện | Kết quả kỳ vọng (Expected) | Kết quả thực tế (Actual) | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| Kiểm tra liên kết nội bộ giữa 7 tài liệu Mục 4 | `10/11/2026` & `11/11/2026` | Tất cả liên kết tương đối giữa các file Markdown/PPTX/PNG hợp lệ | `25/25 PASS`, `missing local targets: 0` (`tests/sprint5/Section4-Link-Check.txt`) | **Pass** |
| Rà soát tính nhất quán số liệu, phạm vi MVP và bảo mật (không lộ secrets) | `10/11/2026` & `11/11/2026` | Không lộ mật khẩu/JWT/connection string; tách rõ số liệu Dev (`128.000đ`) vs Test (`128.020,50đ` / `215.020,50đ`) | Đạt đầy đủ 10 tiêu chí trong bảng rà soát `5.3.6` | **Pass** |
| Rà soát ghép nối đầu vào từ các thành viên khác (`5.3.4` của Khang & 5 báo cáo cá nhân `5.6.6`) | `11/11/2026` | Có bản `5.3.4` của PM Khang và đủ 5 báo cáo cá nhân có xác nhận tác giả | Đang chờ PM Khang bàn giao `5.3.4` và các thành viên gửi đủ báo cáo cá nhân `5.6.1`/`5.6.2`/`5.6.4` | **Blocked** |

---

## 4. Tồn đọng & Phụ thuộc bàn giao
1. **Đầu vào liên thành viên (`Blocked`)**: Chờ bản tổng hợp kiểm thử/UAT/rủi ro `5.3.4` của PM Khang, đủ 5 báo cáo cá nhân (`5.6.6`), và xác minh quyền truy cập 9 link Google/FigJam bằng tài khoản nhóm (`5.3.1`).
2. **Trạng thái task**: Đã hoàn tất toàn bộ 20/20 gói bàn giao của Công Thành trước hạn nội bộ `13/11/2026` và gửi PM Khang review trên Jira, **chưa tự đánh dấu Done**.
