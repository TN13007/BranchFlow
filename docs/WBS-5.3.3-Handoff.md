# WBS-5.3.3 — Viết báo cáo kế hoạch WBS, Gantt và Sprint

Em là Công Thành, có công cụ hỗ trợ; tiếp nhận, đối chiếu số liệu mới nhất và bàn giao **11/11/2026**. Ngày 19/11 trong lịch là kế hoạch. Jira tham chiếu: **SCRUM-107** (Subtask FE liên quan: **SCRUM-127**). Nhánh riêng: `docs/SCRUM-107-wbs-5.3.3-wbs-gantt-sprint-report` tách từ `origin/main` (**ae595fd**).

## Phần tiếp nhận và phần em đóng gói trong nhánh

Em tiếp nhận nguyên vẹn bản báo cáo nội bộ **`docs/Sprint5-WBS-Sprint-Report.md`** đã soạn tại `D:\QLDA\BranchFlow\docs\Sprint5-WBS-Sprint-Report.md` (trùng khớp byte-for-byte SHA256 `2A25ECED8C46DEF604A18825F17107FB3FB1C1F516C82CF282562106AFDFBE78`, Git blob `25742d34f65e8bb28f51156ffa63b4ff97f2df1e`) và bổ sung bảng truy vết trạng thái commit/Jira đến ngày **11/11/2026** trong tài liệu bàn giao này cùng `tests/sprint5/WBS-5.3.3/WBS-Traceability-2026-11-11.txt`.

Các nguyên tắc đối chiếu được giữ nghiêm ngặt:
- **Tách biệt lịch nguồn và hạn nội bộ**: Lịch nguồn của Công Thành gồm **20 hoạt động WBS (40 giờ kế hoạch, 2 giờ/hoạt động)** trải từ `06/11/2026` đến `25/11/2026` (nằm trong khung kế hoạch `06–26/11/2026` của file POD). Hạn nội bộ mới của Sprint 5 là **13/11/2026 23:00 (Asia/Saigon)**. Không biến ngày kế hoạch thành ngày thực tế và không ghi đè Baseline hoặc tự xuất ảnh Gantt giả khi PM chưa xuất lại từ file ProjectLibre chính thức.
- **Không trộn đơn vị số liệu**:
  - Số liệu lịch sử tuần 4 (`29/31` hoạt động gốc Done, `41,10%` theo trọng số estimate nguồn, `103` hoạt động còn lại / `191` giờ kế hoạch nhóm) được giữ đúng mốc báo cáo tuần 4 (`05/11/2026`), không cộng gộp `62` giờ Sprint 4 hay `438` giờ nguồn vào `191` giờ.
  - Kết quả kiểm thử kỹ thuật được tách rõ theo từng đơn vị: Build Release (`0 Warning, 0 Error`), Unit test không Integration (`76/76 Pass`), Full SQL-enabled suite (`81/81 Pass` trên nhánh retest của Thành sau sửa clock 2 test; `83/83 Pass` trên nhánh QA merge `554bea9` ngày `11/11/2026`), HTTP checks (`62` checks trong `Verify-FrontendApis.ps1`, `23` checks trong `Verify-WbsDemoData.ps1`, `72/72` trong `ManagementSmoke`, `45/45` trong `OrderAdjustmentSmoke`, `11/11` trong `RevenueSmoke`). Không cộng gộp số dòng kịch bản UAT hay số assertion script vào số test case catalog Sprint 4 (`44` ca).
- **Tách biệt kết quả local và nghiệm thu public**: Toàn bộ minh chứng hiện tại là kết quả chạy thật trên môi trường local (`BranchFlow_Test` / `BranchFlow_Dev`); smoke trên URL public HTTPS vẫn ở trạng thái `NotRun` (chờ PM Khang bàn giao URL HTTPS, deployed SHA và release manifest).

## Bảng truy vết 20 WBS của Công Thành (cập nhật 11/11/2026)

| STT | Ngày KH | Mã WBS | Tên hoạt động | Giờ KH | Nhánh / Commit GitHub | Jira tham chiếu / Comment | Trạng thái bàn giao |
|---:|---|---|---|---:|---|---|---|
| 1 | 06/11 | `5.1.1` | Tạo dữ liệu mẫu 3 chi nhánh và 4 vai trò | 2h | `feat/SCRUM-127-wbs-5.1.1-handoff` (`b7184c7`, đã merge `main`) | `SCRUM-127` | Đã bàn giao & merge `main` |
| 2 | 07/11 | `3.4.2.1` | Hiển thị yêu cầu điều chỉnh theo chi nhánh | 2h | `feat/SCRUM-124-wbs-3.4.2.1-handoff` (`a556bcc` / `4d14af2` / `e2db2b5`) | `SCRUM-124` | Đã bàn giao; chờ review/merge |
| 3 | 08/11 | `4.4.1` | Chuẩn bị kịch bản UAT theo 4 vai trò | 2h | `docs/SCRUM-131-wbs-4.4.1-thanh-uat` (`4b04b7f`) | `SCRUM-131` | Đã bàn giao; chờ QA UAT 4 vai trò |
| 4 | 09/11 | `5.1.2` | Tạo dữ liệu mẫu menu và đơn hàng | 2h | `feat/SCRUM-127-wbs-5.1.2-demo-menu` (`6f99364`, đã merge `main`) | `SCRUM-127` | Đã bàn giao & merge `main` |
| 5 | 10/11 | `3.6.2.3` | Lọc ngày/chi nhánh theo quyền | 2h | `feat/SCRUM-126-wbs-3.6.2.3-revenue` (`1488e9e`, đã merge `main`) | `SCRUM-126` | Đã bàn giao & merge `main` |
| 6 | 11/11 | `3.6.2.4` | Xử lý trạng thái trống/lỗi và số liệu | 2h | `docs/SCRUM-126-wbs-3.6.2.4-revenue-states` (`cf6ef7b`, đã merge `main`) | `SCRUM-126` | Đã bàn giao & merge `main` |
| 7 | 12/11 | `5.1.3` | Tạo dữ liệu mẫu tồn kho và doanh thu | 2h | `docs/SCRUM-127-wbs-5.1.3-stock-revenue` (`095fdbf`, đã merge `main`) | `SCRUM-127` | Đã bàn giao & merge `main` |
| 8 | 13/11 | `3.5.1.5` | Xem tồn kho / phiếu giao dịch | 2h | `feat/SCRUM-125-wbs-3.5.1.5-stock-ui` (`1e2a678`, đã merge `main`) | `SCRUM-125` | Đã bàn giao & merge `main` |
| 9 | 14/11 | `4.6.2` | Kiểm thử responsive ở 360/768/1366px | 2h | `test/SCRUM-127-wbs-4.6.2-responsive` (`fe9a0e4`) | `SCRUM-127` (Comment `10300`) | Đã bàn giao; chờ review |
| 10 | 15/11 | `4.6.5` | Đánh giá tương thích Chrome/Edge/Android | 2h | `docs/SCRUM-127-wbs-4.6.5-browser-compatibility` (`96d8763`) | `SCRUM-127` (Comment `10301`) | Đã bàn giao (Android/Public `NotRun`) |
| 11 | 16/11 | `3.2.2.5` | Hiển thị hồ sơ nhân viên và kiểm quyền Owner chỉ đọc | 2h | `feat/SCRUM-122-wbs-3.2.2.5-employee-profiles` (`10ba6e0`) | `SCRUM-122` (Comment `10302`) | Đã bàn giao UI/HTTP; Owner đọc hồ sơ `Blocked` chờ API |
| 12 | 17/11 | `3.4.3.1` | Tạo màn hình lịch sử đơn do Employee lập | 2h | `docs/SCRUM-124-wbs-3.4.3.1-employee-history` (`d4d2696`) | `SCRUM-124` (Comment `10303`) | Đã bàn giao; chờ review |
| 13 | 18/11 | `3.3.1.5` | Tạo giao diện quản lý menu tổng | 2h | `feat/SCRUM-123-wbs-3.3.1.5-system-menu-ui` (`7f53486`) | `SCRUM-123` (Comment `10304`) | Đã bàn giao; chờ QA `3.3.1.6` |
| 14 | 19/11 | `5.3.3` | Viết báo cáo kế hoạch WBS, Gantt và Sprint | 2h | `docs/SCRUM-107-wbs-5.3.3-wbs-gantt-sprint-report` | `SCRUM-107` | Đang bàn giao trong gói này |
| 15–20 | 20–25/11 | `5.2.3`, `5.6.3`, `5.3.5`, `5.3.1`, `5.6.6`, `5.3.6` | User Guide, Mini-PM Thành, Slide demo, Hồ sơ tuần 2, Gộp 5 báo cáo, Rà soát cuối | 12h | Đã có bản thảo tại `D:\QLDA\BranchFlow\docs\`; đang lần lượt tách nhánh bàn giao | `SCRUM-127` / `SCRUM-107` / `SCRUM-108` | Chuẩn bị bàn giao |

## Case / expected / actual

| Ca kiểm chứng | Expected | Actual / Nguồn | Kết quả |
|---|---|---|---|
| Đối chiếu 20 hoạt động WBS (40h kế hoạch) và chuỗi phụ thuộc Gantt của Công Thành | Đủ 20 mã WBS, đúng tên đầu ra, giờ kế hoạch (2h/mục) và chuỗi phụ thuộc (`5.1.1→5.1.2→5.1.3→5.5.2`, `3.6.2.3→4.6.2→4.6.5`, `5.2.2→5.2.3→5.2.4`, `5.6.1–5.6.5→5.6.6→5.3.6`) | Khớp bảng lịch nguồn trong `docs/Sprint5-WBS-Sprint-Report.md` và `tests/sprint5/WBS-5.3.3/WBS-Traceability-2026-11-11.txt` | **Pass** |
| Nhất quán số liệu báo cáo tuần 4, POD và kết quả kiểm thử Sprint 5 | Không trộn estimate với actual; không cộng gộp unit/HTTP assertion vào ca catalog; ghi rõ chênh lệch doanh thu Test trước/sau retest (`128.020,50đ` vs `215.020,50đ`, fixture tĩnh `128.000đ`) | Đã ghi chú rõ ở đầu `docs/Sprint5-WBS-Sprint-Report.md` và đối chiếu với `Sprint5-FE-Section1-handoff.md`, `Sprint5-Demo-Data.md`, `Sprint5-UAT-Responsive.md`, `Sprint5-Retest-Completion.md` | **Pass** |
| Tách bạch đầu ra local và nghiệm thu public | Ghi rõ mọi kết quả hiện có là local (`BranchFlow_Test`/`BranchFlow_Dev`), không tuyên bố đã nghiệm thu public | Đã tách rõ trong báo cáo và bảng trạng thái | **Pass** |
| Xuất biểu đồ Gantt mới từ ProjectLibre & đồng bộ trạng thái cuối trên Jira/Word tổng | PM xuất từ file `.pod` chính thức và ghép vào báo cáo tổng của nhóm | Chờ PM **Khang** thực hiện trên file `.pod` và tài liệu tổng | **NotRun / Chờ PM** |

## Chưa đạt / phụ thuộc / người nhận

- **Người nhận**: PM **Khang** nhận `docs/Sprint5-WBS-Sprint-Report.md` và `docs/WBS-5.3.3-Handoff.md` để đối chiếu WBS, ghép số liệu phần FE vào báo cáo tổng Sprint 5 và xuất ảnh Gantt chính thức từ ProjectLibre (`.pod`).
- **Tồn đọng chung cần PM điều phối trước hạn 13/11**:
  1. Chốt quyền/endpoint cho Owner đọc hồ sơ nhân viên (`3.2.2.5` / `SCRUM-122`).
  2. Triển khai bản public HTTPS, cung cấp URL/deployed SHA để QA chạy smoke public (`SCRUM-115` / `SCRUM-133`).
  3. Hoàn tất QA retest độc lập và thu thập đủ 5 báo cáo cá nhân (`5.6.1–5.6.5`).
