# WBS-4.4.1 — Phản hồi và phần còn chờ

Em ghi nhận ngày10/11/2026 từ lượt tự kiểm local; đây không phải phản hồi có chữ ký của ba người dùng hoặc defect mới đã được QA xác nhận.

| Mã | Ghi nhận | Tác động / đề xuất ưu tiên | Tình trạng và người cần xử lý |
|---|---|---|---|
| F01 / U26 | Owner GET danh sách nhân viên trả403; contract/quyền đọc chưa chốt | Chặn riêng kịch bản Owner đọc hồ sơ; cần PM quyết định phạm vi | Blocked; Thuần/Khang chốt, Thành nối/retest sau. Chưa tự kết luận là bug khi contract chưa rõ |
| F02 | Ô chọn tài khoản phân công bị cắt ở768px | Cản chọn tài khoản trên tablet | Đã sửa/retest trong working tree local trước; chưa suy ra đã tích hợp candidate phát hành. Thuận kiểm lại đúng SHA; code CSS thuộc WBS responsive, không đưa vào commit4.4.1 |
| F03 | Menu Employee bị cắt giỏ/cột món ở360px | Cản thao tác mobile | Đã sửa/retest local trước; cần review/integrate và QA đúng candidate. Không dùng viewport360 để nhận đã test Android thật |
| F04 | POST401/timeout sau commit có nguy cơ gửi lại trùng | Cần người dùng kiểm lịch sử trước quyết định gửi lại | Hai ca local U27/U28 đã quan sát giữ giỏ/cảnh báo/khóa gửi; không suy mọi endpoint kho có cùng kết quả |
| F05 | Chưa có URL Web/API và deployedSHA | Chưa chạy public | Khang bàn giao đầu vào; Thuận smoke public sau deploy |
| F06 | Chưa có ba người dùng/QA độc lập xác nhận trong bộ Thành | Chưa có nghiệm thu nhóm | Hòa Thuận nhận đầu ra4.4.3, tổ chức lượt riêng và điền actual; Khang review. Thành không ký thay |
| F07 | Candidate ảnh cũ là47541ee+working tree, chưa là SHA phát hành sạch | Không thể gọi ảnh cũ là retest trên một SHA mới | Giữ provenance; QA phải ghi SHA/build thật khi chạy candidate cuối |

Chưa tạo Jira Bug mới từ bảng này. Khi tái hiện lỗi trên candidate chốt, ghi issue key, bước/lần chạy, severity/priority được nhóm thống nhất, expected/actual, ảnh/log và SHA sửa/retest. Hai lỗi responsive là kết quả local đã có, không tự tạo trùng bug.
