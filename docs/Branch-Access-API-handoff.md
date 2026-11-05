# Handoff API quyền người dùng và phạm vi chi nhánh

## Endpoints

### GET /api/auth/me

Yêu cầu Bearer JWT hợp lệ. Endpoint lấy `sub` từ token đã xác thực và trả về người dùng đang hoạt động:

```json
{
  "userId": 17,
  "username": "demo.employee.a",
  "role": "EMPLOYEE"
}
```

Không nhận userId hoặc role từ query/body. Không trả password hash hoặc thông tin xác thực.

### GET /api/branches/accessible

Yêu cầu Bearer JWT hợp lệ. Endpoint trả danh sách chi nhánh mà người dùng hiện tại được truy cập:

```json
[
  {
    "id": 1,
    "code": "BF-DEMO-01",
    "name": "Chi nhánh demo 01"
  }
]
```

Người dùng không có phân công còn hiệu lực nhận mảng rỗng `[]`.

## Quy tắc branch scope dùng chung

Inject `IBranchAccessService` từ `BranchFlow.Application.Services` vào service nghiệp vụ cần giới hạn phạm vi:

- `GetAccessibleBranchesAsync(userId, cancellationToken)`: lấy danh sách branch theo role và assignment hiện tại.
- `CanAccessBranchAsync(userId, branchId, cancellationToken)`: kiểm tra branch có nằm trong phạm vi truy cập hiện tại hay không.
- `HasBranchScopeAsync(userId, branchId, cancellationToken)`: kiểm tra role/phân công hiệu lực, không yêu cầu chi nhánh active. Menu dùng phép kiểm này trước, sau đó repository menu kiểm tra trạng thái chi nhánh: ngoài phạm vi trả 403; trong phạm vi nhưng inactive/deleted trả 404. Kết quả true không cho phép tạo đơn trên chi nhánh ngừng hoạt động.

Quy tắc do repository dùng chung thực hiện:

| Role | Phạm vi truy cập |
|---|---|
| OWNER | Tất cả branch đang active và chưa deleted |
| SYSTEM_ADMIN | Tất cả branch đang active và chưa deleted |
| BRANCH_ADMIN | Branch có AdminBranchAssignment hiện hiệu lực |
| EMPLOYEE | Branch có EmployeeBranchAssignment hiện hiệu lực và EmployeeProfile chưa deleted |

Assignment tuân theo khoảng UTC nửa mở `[EffectiveFrom, EffectiveTo)`: bắt đầu có hiệu lực khi `EffectiveFrom <= now`; kết thúc hiệu lực khi `EffectiveTo <= now`. Danh sách accessible và `CanAccessBranchAsync` chỉ nhận branch `IsActive = 1`, `Deleted = 0`; `HasBranchScopeAsync` giữ phân công độc lập với trạng thái branch. Mọi truy vấn lấy phạm vi dựa trên userId đã xác thực ở server, không tin userId/role do client gửi.

Ví dụ kiểm tra trước khi tạo nghiệp vụ theo chi nhánh:

```csharp
if (!await _branchAccessService.CanAccessBranchAsync(
    userId,
    request.BranchId,
    cancellationToken))
{
    throw new UnauthorizedAccessException();
}
```

Controller nghiệp vụ vẫn phải khai báo role được phép bằng `[Authorize(Roles = "...")]`. Branch scope không thay thế role authorization. Ví dụ menu/đơn hàng chỉ cho EMPLOYEE; OWNER/SYSTEM_ADMIN không được tạo đơn chỉ vì họ xem được danh sách branch.

## JWT và session validation

JWT middleware xác minh chữ ký HS256, issuer, audience, lifetime và các claim `sub`, `sid`, `role`. Mỗi request đã xác thực truy vấn session trong DB và kiểm tra đồng thời:

- session ID khớp `sid`, user ID khớp `sub`;
- session chưa bị revoke, chưa hết hạn và `CreatedAt <= UTC now`;
- user đang active, chưa deleted;
- role trong DB khớp role trong token và thuộc một trong bốn role hỗ trợ.

Do đó access JWT bị từ chối sau khi session bị thu hồi, không chỉ chờ chữ ký JWT hết hạn.

## HTTP status

- 401: thiếu/sai/hết hạn JWT; claim không hợp lệ; session hết hạn/revoked; user bị khóa/xóa.
- 403: JWT và session hợp lệ nhưng endpoint không cho role đó hoặc branch nằm ngoài scope.
- GET accessible không có assignment: 200 với `[]`, không phải 403.

Challenge và forbid trả ProblemDetails JSON có `traceId`. Không ghi access token, refresh token, token hash, JWT key hoặc connection string vào log.

## Handoff cho BE phụ trách menu/order

- Đăng ký và inject `IBranchAccessService`; không tự viết lại quy tắc assignment ở controller.
- Lấy userId từ claim `sub`; role authorization từ claim `role` bằng `[Authorize(Roles = "...")]`.
- Kiểm tra `CanAccessBranchAsync` tại server trước khi query/tạo dữ liệu.
- Employee A thao tác branch B phải bị 403 và không tạo dữ liệu.
- Owner/System Admin không được route tạo đơn nếu contract chỉ cho EMPLOYEE.
- Không dựa vào việc FE ẩn nút hoặc không gửi branchId. Luôn test trực tiếp HTTP.
