# Phân công file Markdown và quy trình commit – Nhóm 6

**Mục tiêu:** Mỗi người có trách nhiệm cụ thể đối với tài liệu Tuần 1, hiểu nội dung đã làm và có **ít nhất một đóng góp GitHub thực chất**. Phân công dưới đây là **kế hoạch thực hiện**, không tự khẳng định ai đã commit.

## 1. Người phụ trách chính và reviewer

| Thành viên | File cần chịu trách nhiệm chính | Công việc cần thực hiện thực chất | Người rà soát đề xuất |
|---|---|---|---|
| **Bá Thành Chí Khang (PM/BA)** | `docs/project-charter.md`, `docs/topic-selection.md`, `docs/evidence-assumptions.md` | Kiểm tra 16 mục Charter, cập nhật chính xác nguồn quan sát, quyết định phạm vi và giả định; sửa các nhận định không khớp thực tế. | QA/Documentation |
| **Nguyễn Anh Trường Tài (UI/UX + Dev)** | `docs/problem-statement.md`, `src/README.md` | Làm rõ thao tác người dùng, mô tả nhu cầu Employee, kiểm tra nội dung bán hàng/menu, góp ý UI mobile-first; cập nhật hướng source. | PM/BA |
| **Nguyễn Công Thành (UI/UX + Dev)** | `README.md`, `docs/stakeholder-register.md` | Rà soát giới thiệu dự án, role/branch scope và quyền Owner/Admin/Employee; đối chiếu stakeholder với cơ chế học phần. | PM/BA |
| **Nguyễn Phước Thuần (UI/UX + Dev)** | `planning/week-01-plan.md`, `docs/change-request-scenario.md` | Đồng bộ người phụ trách/deadline với Jira, rà soát kế hoạch và phân tích tác động thay đổi phạm vi; liên hệ nghiệp vụ kho/lương/báo cáo. | PM/BA hoặc QA |
| **Nguyễn Đặng Hòa Thuận (QA/Documentation + hỗ trợ Dev)** | `docs/team-charter.md`, `planning/week-01-summary.md`, `tests/README.md`, `docs/ai-usage-disclosure.md`, `planning/contribution-plan.md` | Đối chiếu cam kết của 5 người; cập nhật kết quả Tuần 1 theo sự thật; viết hướng test, checklist tài liệu, quy tắc commit; kiểm tra nội dung khai báo AI. | PM/BA |

**Cân bằng khối lượng:** Một người có thể sở hữu nhiều file ngắn hơn hoặc ít file dài hơn. Số lượng file/commit **không** phải tiêu chí duy nhất để đánh giá đóng góp; ưu tiên mức độ thay đổi và sự hiểu biết.

## 2. Cách commit (mỗi người thao tác bằng tài khoản GitHub riêng)

### Cách được khuyến nghị: Branch + Pull Request

1. Thành viên clone repository bằng tài khoản GitHub của mình (đã được nhóm cấp quyền phù hợp).
2. Tạo nhánh riêng theo tên hoặc vai trò; chỉ sửa các file được giao và đọc đủ ngữ cảnh.
3. **Tự kiểm tra, sửa và bổ sung nội dung có ý nghĩa.** Không chỉ tải bản AI rồi commit nguyên xi để đủ tên.
4. Commit với thông điệp mô tả chính xác nội dung đã thay đổi.
5. Push nhánh và tạo Pull Request vào `main`.
6. Reviewer kiểm tra, trao đổi và merge sau khi thống nhất. Gắn link PR/commit vào task Jira tương ứng.

```bash
git clone https://github.com/TN13007/BranchFlow.git
cd BranchFlow
git switch -c docs/<ten-hoac-ma-thanh-vien>-week1
# Chỉnh sửa các file được giao; kiểm tra git diff trước khi lưu
git status
git diff
git add docs/project-charter.md  # Thay bằng đường dẫn file bản thân chịu trách nhiệm
git commit -m "docs: refine BranchFlow project charter scope and assumptions"
git push -u origin HEAD
```

> Chú ý: Khi dùng **GitHub Web**, có thể bấm biểu tượng chỉnh sửa file, lưu thay đổi vào một nhánh mới, sau đó tạo Pull Request; không bắt buộc dùng command line. Không cần số commit nhiều, nhưng phải có nội dung thật và người chịu trách nhiệm hiểu được.

## 3. Thông điệp commit đề xuất (chỉ dùng sau khi thật sự chỉnh sửa)

| Thành viên | Ví dụ commit message |
|---|---|
| Khang | `docs: validate topic selection and project charter scope` |
| Tài | `docs: clarify employee workflow and problem statement` |
| Thành | `docs: refine README and stakeholder responsibilities` |
| Thuần | `docs: align sprint plan and analyze scope change scenario` |
| Thuận | `docs: review team charter, testing approach and week1 summary` |

## 4. Checklist review trước merge

- [ ] Nội dung thật, không gán dữ liệu khảo sát hoặc số liệu doanh thu giả.
- [ ] Mục tiêu/phạm vi/thời gian/nhân sự khớp với Project Charter và bản báo cáo Word.
- [ ] Markdown hiển thị tốt trên GitHub: bảng, link, danh sách, tiêu đề.
- [ ] Không đưa password, token, email riêng tư, thông tin khách hàng hoặc dữ liệu cửa hàng thật chưa được phép.
- [ ] Reviewer hiểu thay đổi và xác nhận trên PR.
- [ ] Gắn liên kết PR/commit vào task Jira tương ứng.
- [ ] Khi sửa tài liệu trên GitHub, đồng bộ các thay đổi quan trọng vào bản Word để nộp.

## 5. Yêu cầu tối thiểu của Tuần 1 theo tài liệu giảng viên

Mỗi thành viên cần **ít nhất một đóng góp thực chất**, chẳng hạn sửa tài liệu phân tích, bổ sung mô tả chức năng/đối tượng hoặc sửa sai có căn cứ. **Không đạt** nếu chỉ tạo thư mục rỗng, đổi dấu câu không có ý nghĩa hoặc commit file mà người đó chưa đọc và xác nhận.
