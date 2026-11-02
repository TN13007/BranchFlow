# Kết quả kiểm chứng baseline v1

Ngày thực hiện: 02/11/2026. Agent đã thực thi trên SQL Server local Windows Authentication, server version 17.0.1000.7. Script dùng cú pháp SQL Server 2019+; chưa chạy thử trên engine 2019 hoặc Azure SQL.

## Đã thực hiện

- Tạo BranchFlow_Dev và BranchFlow_Test khi cả hai tên DB đều chưa tồn tại; không đổi DB cá nhân/cloud.
- Chạy 001-baseline.sql và 002-reference-data.sql trên hai DB. Dev giữ đúng 10 dòng nền: 4 Role, 2 Size, 4 IngredientUnit. Chưa có user/đơn/menu mẫu.
- Kiểm tra từ chối chạy baseline trong master và từ chối chạy đè DB đã có schema. Hai lần đều trả lỗi 51000 trước thay đổi schema.
- Đối chiếu SQL metadata với thiết kế: đủ 35 bảng, 271 cột, 59 FK; toàn bộ kiểu/nullability/computed/identity và đích FK khớp schema-v1.json. FK được enable/trusted và không cascade delete.
- Chạy database/Verify-Database.ps1 thành công: constraint SQL thật, uniqueness, soft-delete employee identity, giá/tồn/lương không âm, topping 0/4 bị từ chối, giới hạn phân công mở, khoảng thời gian, bảo vệ lịch sử qua FK, trạng thái đơn, stale RowVersion, computed lương và chênh lệch kiểm kê decimal(18,3), hash refresh duy nhất.
- Counter reset đúng output cũ, tăng tuần tự, giữ đầy đủ mã sau 9999; fixture/order/counter cùng rollback. 12 SQL connections chạy đồng thời nhận 12 code duy nhất, counter cuối 12; fixture concurrency được dọn đúng Id/code.
- Scaffold bằng dotnet-ef 10.0.11 vào Infrastructure, --no-onconfiguring, không --force. Đủ 35 model; DbContext có 35 entity, 271 property, 59 relationship. Không có connection string hoặc OnConfiguring trong source sinh.
- Probe EF Core riêng ngoài repo kết nối Dev, truy vấn cả 35 bảng và đọc đúng reference seed; không có user/order fixture trong Dev. Probe không phải endpoint nghiệp vụ hoặc test xUnit đã bàn giao.
- Build toàn solution Release thành công, 0 warning/0 error.
- Chạy API Development bằng User Secrets, truy cập HTTPS Swagger trả HTTP 200, sau đó dừng host do agent mở. Swagger thành công chỉ chứng minh startup, không chứng minh API nghiệp vụ đã có.

## Phạm vi chưa triển khai

Chưa có endpoint/service/repository nghiệp vụ, UI gọi dữ liệu thật, seed tài khoản demo, auth JWT/refresh runtime, kiểm tra quyền branch ở API, logic duyệt kiểm kê/điều chuyển hoặc deploy. Những nghiệp vụ cần service được ghi rõ tại README trong thư mục này. Không dùng kết quả kiểm tra DB để chuyển các Jira task đó sang Done.

## Cách chạy lại

Trên BranchFlow_Test đã có baseline và reference seed:

```powershell
./database/Verify-Database.ps1 -Server localhost
```

Sau khi sửa source/config build, chạy lại build tương ứng. Không chạy SQL kiểm thử lên Dev hoặc cloud. Không seed/reset DB có dữ liệu để tạo minh chứng.
