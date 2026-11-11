# Bàn giao WBS 5.6.6 — Gộp 5 báo cáo cá nhân và kiểm tra minh chứng

Em là Công Thành, có công cụ hỗ trợ, tổng hợp và bàn giao khung tiếp nhận 5 báo cáo cá nhân cho **WBS 5.6.6** (Story tham chiếu `SCRUM-107`).

## 1. Thông tin chung
- **WBS**: `5.6.6` — Gộp 5 báo cáo cá nhân và kiểm tra minh chứng (`BF-NB-5.6.6`)
- **Người thực hiện**: Nguyễn Công Thành (FE & hỗ trợ tài liệu)
- **Jira tham chiếu**: `SCRUM-107` (`https://khangba153.atlassian.net/browse/SCRUM-107`)
- **Lịch kế hoạch ProjectLibre**: `2026-11-24 14:00 -> 2026-11-24 16:00` (2h) — Hạn nội bộ mới: **Thứ Sáu, 13/11/2026 23:00**
- **Ngày soạn thảo bản gốc**: `10/11/2026` (trên working tree `thanhnc`, nền `47541ee`)
- **Ngày kiểm chứng định dạng & đóng gói bàn giao**: `11/11/2026` (trên nhánh sạch `docs/SCRUM-107-wbs-5.6.6-five-personal-reports` tách từ `origin/main` `ae595fd3872167a70aa9e3f537601682c31e2e28`)
- **Tiền nhiệm theo lịch**: `5.6.1` (Khang), `5.6.2` (Trường Tài), `5.6.3` (Công Thành), `5.6.4` (Phước Thuần), `5.6.5` (Hòa Thuận)
- **Người nhận bàn giao tiếp theo**: `5.3.6` (Công Thành) và PM Bá Thành Chí Khang (`SCRUM-107`)

---

## 2. Đầu ra đã thực hiện
1. **Tài liệu tiếp nhận và đối chiếu 5 báo cáo cá nhân (`docs/Sprint5-Five-Reports-Index.md`)**:
   - Giữ nguyên bản gốc soạn ngày `10/11/2026` (SHA256 `2DCB37D1FC065D8D591ADD431D3A3D954FB13CC17393C1CF631E7664D41C198F`, Git blob `00cb038afc435ecf0785e9047bcacf5f5e678586`).
   - Lập bảng trạng thái tiếp nhận 5 báo cáo cá nhân (`5.6.1`–`5.6.5`):
     - **`5.6.1` (Bá Thành Chí Khang)**: Chờ bản báo cáo cá nhân riêng và xác nhận của tác giả.
     - **`5.6.2` (Nguyễn Anh Trường Tài)**: Chờ bản báo cáo cá nhân riêng và xác nhận của tác giả.
     - **`5.6.3` (Nguyễn Công Thành)**: Đã có [`docs/Sprint5-MiniPM-Thanh.md`](Sprint5-MiniPM-Thanh.md) (đã nộp ở branch `docs/SCRUM-107-wbs-5.6.3-minipm-thanh`, commit `39de709`).
     - **`5.6.4` (Nguyễn Phước Thuần)**: Chờ bản báo cáo cá nhân riêng và xác nhận của tác giả.
     - **`5.6.5` (Nguyễn Đặng Hòa Thuận)**: Đã có báo cáo QA `tests/sprint4/MiniPMReport-2026-11-09.md` trong repo (kèm comment của Hòa Thuận trên `SCRUM-107`); chờ xác nhận bản cuối `WBS 5.6.5`.
   - **Quy tắc chống đếm trùng và không ký thay**:
     - Không cộng gộp số lượng test giữa báo cáo của Hòa Thuận (`81/81` SQL-enabled suite trên candidate `e83ccb8`/`a293ddf`) với lượt chạy của Công Thành (`76` unit test + `62` HTTP assertions).
     - Không tự tạo nội dung đóng góp, không suy diễn `Actual Work` từ `Planned Hours` và không ký xác nhận thay cho các thành viên khác.

---

## 3. Kết quả tự kiểm chứng (`11/11/2026` vs `10/11/2026`)

| Hạng mục kiểm chứng | Ngày thực hiện | Kết quả kỳ vọng (Expected) | Kết quả thực tế (Actual) | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| Khung đối chiếu 5 báo cáo cá nhân (`5.6.1`–`5.6.5`) | `10/11/2026` & `11/11/2026` | Liệt kê đủ 5 mã WBS, người phụ trách, nguồn đã có trong repo và tình trạng thực tế | Đủ 5 dòng (`5.6.1`–`5.6.5`), liên kết đúng tới `Sprint5-MiniPM-Thanh.md` và `tests/sprint4/MiniPMReport-2026-11-09.md` | **Pass** |
| Quy tắc không đếm trùng effort/test và không ký thay | `10/11/2026` & `11/11/2026` | Tách biệt candidate/fixture/thời điểm chạy test giữa các thành viên; không tự điền Actual Hours | Ghi rõ nguyên tắc tách biệt và điều kiện chốt bản gộp | **Pass** |
| Nhận đủ 5 bản báo cáo cá nhân có xác nhận tác giả | `11/11/2026` | Đủ 5/5 bản cuối từ Khang (`5.6.1`), Tài (`5.6.2`), Thành (`5.6.3`), Thuần (`5.6.4`), Thuận (`5.6.5`) | Mới có đầy đủ bản `5.6.3` của Thành và bản `09/11` của Thuận; còn chờ `5.6.1`, `5.6.2`, `5.6.4` và xác nhận cuối | **Blocked** |

---

## 4. Tồn đọng & Phụ thuộc bàn giao
1. **Đầu vào đang chờ (`Blocked`)**: Chờ các bản Mini-PM cá nhân của Khang (`5.6.1`), Trường Tài (`5.6.2`), Phước Thuần (`5.6.4`) và xác nhận bản cuối `5.6.5` của Hòa Thuận để gộp thành bản 5 người hoàn chỉnh.
2. **Trạng thái task**: Bàn giao khung tiếp nhận và đối chiếu minh chứng lên `SCRUM-107`, **chưa tự đánh dấu Done**.
