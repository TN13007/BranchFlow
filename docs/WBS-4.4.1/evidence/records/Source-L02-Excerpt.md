# Trích nguồnL02 — không phải lượt mới

Nguồn:Sprint5-Retest-Completion.md; SHA256: 25971FAAFB36363F0B46C56D86B815A2C1D9D830B64C7EB60B158F529C97E91C
Candidate47541ee+working tree; Công Thành,10/11/2026.

| U25: xác nhận hai lần | Chrome gửi clickCount=2 trên nút xác nhận giỏ 1 ly 29.000đ; SQL tăng từ 10 lên 11 đơn, chỉ tạo ID25/mã0010 | Pass trong lượt đã chạy; không suy ra API có idempotency |
| 401 sau commit đơn | Web17133 qua proxy17108 tới API17107; proxy chuyển response201 thành401 sau khi upstream lưu. Có đúng một committed POST được proxy ghi nhận; SQL ID26/mã0011,29.000đ | Pass |
| Giỏ sau mất phiên | Giao diện báo chưa xác định kết quả, giữ1ly, nút xác nhận bị khóa. Sau đăng nhập lại cùng tài khoản, giỏ/cảnh báo vẫn giữ; lịch sử có mã0011 | Pass; không bấm xác nhận chưa tạo vì đơn thực sự đã tồn tại |
| Timeout sau commit đơn | Web17134 qua proxy17109; proxy giữ response201 trong30s, Web timeout25s. Đang gửi khóa thao tác; sau timeout giữ giỏ, cảnh báo chưa rõ kết quả và khóa gửi. Lịch sử hiển thị ID27/mã0012; proxy ghi nhận một committed POST | Pass |