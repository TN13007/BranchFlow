using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;

namespace BranchFlow.Application.Services;

public sealed class DemoSeedService : IDemoSeedService
{
    private const string OwnerRoleCode = "OWNER";
    private const string SystemAdminRoleCode = "SYSTEM_ADMIN";
    private const string BranchAdminRoleCode = "BRANCH_ADMIN";
    private const string EmployeeRoleCode = "EMPLOYEE";
    private const int MinimumPasswordLength = 12;

    private readonly IDemoSeedRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly TimeProvider _clock;

    public DemoSeedService(
        IDemoSeedRepository repository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<AppUser> passwordHasher,
        TimeProvider clock)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    // Điều phối toàn bộ quá trình seed trong một transaction.
    public async Task<DemoSeedResult> SeedAsync(
        string password,
        CancellationToken cancellationToken = default)
    {
        ValidatePassword(password);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            DateTime now = _clock.GetUtcNow().UtcDateTime;

            Role ownerRole = await GetRequiredRoleAsync(OwnerRoleCode, cancellationToken);
            Role systemAdminRole = await GetRequiredRoleAsync(SystemAdminRoleCode, cancellationToken);
            Role branchAdminRole = await GetRequiredRoleAsync(BranchAdminRoleCode, cancellationToken);
            Role employeeRole = await GetRequiredRoleAsync(EmployeeRoleCode, cancellationToken);

            Branch branch01 = await EnsureBranchAsync(
                "BF-DEMO-01",
                "Chi nhánh Demo 01",
                "Địa chỉ thử nghiệm 01 - không dùng giao hàng",
                now,
                cancellationToken);
            Branch branch02 = await EnsureBranchAsync(
                "BF-DEMO-02",
                "Chi nhánh Demo 02",
                "Địa chỉ thử nghiệm 02 - không dùng giao hàng",
                now,
                cancellationToken);
            Branch branch03 = await EnsureBranchAsync(
                "BF-DEMO-03",
                "Chi nhánh Demo 03",
                "Địa chỉ thử nghiệm 03 - không dùng giao hàng",
                now,
                cancellationToken);

            AppUser owner = await EnsureUserAsync(
                "demo.owner", "Demo Owner", ownerRole, true, password, now, cancellationToken);
            AppUser systemAdmin = await EnsureUserAsync(
                "demo.systemadmin", "Demo System Admin", systemAdminRole, true, password, now, cancellationToken);

            AppUser branchAdmin01 = await EnsureUserAsync(
                "demo.branchadmin.01", "Demo Branch Admin 01", branchAdminRole, true, password, now, cancellationToken);
            AppUser branchAdmin02 = await EnsureUserAsync(
                "demo.branchadmin.02", "Demo Branch Admin 02", branchAdminRole, true, password, now, cancellationToken);
            AppUser branchAdmin03 = await EnsureUserAsync(
                "demo.branchadmin.03", "Demo Branch Admin 03", branchAdminRole, true, password, now, cancellationToken);

            AppUser employeeA = await EnsureUserAsync(
                "demo.employee.a", "Demo Employee A", employeeRole, true, password, now, cancellationToken);
            AppUser employeeB = await EnsureUserAsync(
                "demo.employee.b", "Demo Employee B", employeeRole, true, password, now, cancellationToken);
            AppUser lockedEmployee = await EnsureUserAsync(
                "demo.employee.locked", "Demo Locked Employee", employeeRole, false, password, now, cancellationToken);
            AppUser unassignedEmployee = await EnsureUserAsync(
                "demo.employee.unassigned", "Demo Unassigned Employee", employeeRole, true, password, now, cancellationToken);

            // Lưu trước để SQL Server cấp ID cho chi nhánh và tài khoản mới.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            EmployeeProfile employeeAProfile = await EnsureEmployeeProfileAsync(
                employeeA, "DEMO-EMP-A", now, cancellationToken);
            EmployeeProfile employeeBProfile = await EnsureEmployeeProfileAsync(
                employeeB, "DEMO-EMP-B", now, cancellationToken);
            EmployeeProfile lockedEmployeeProfile = await EnsureEmployeeProfileAsync(
                lockedEmployee, "DEMO-EMP-LOCKED", now, cancellationToken);
            await EnsureEmployeeProfileAsync(
                unassignedEmployee, "DEMO-EMP-UNASSIGNED", now, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // SYSTEM_ADMIN là người tạo phân công quản trị chi nhánh.
            await EnsureAdminAssignmentAsync(
                branch01, branchAdmin01, systemAdmin, now, cancellationToken);
            await EnsureAdminAssignmentAsync(
                branch02, branchAdmin02, systemAdmin, now, cancellationToken);
            await EnsureAdminAssignmentAsync(
                branch03, branchAdmin03, systemAdmin, now, cancellationToken);

            // Mỗi nhân viên được gán bởi quản trị viên của chi nhánh tương ứng.
            await EnsureEmployeeAssignmentAsync(
                employeeAProfile, branch01, branchAdmin01, now, cancellationToken);
            await EnsureEmployeeAssignmentAsync(
                employeeBProfile, branch02, branchAdmin02, now, cancellationToken);
            await EnsureEmployeeAssignmentAsync(
                lockedEmployeeProfile, branch03, branchAdmin03, now, cancellationToken);

            // demo.employee.unassigned có hồ sơ nhưng cố ý không có phân công chi nhánh.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            DemoSeedResult result = new(
                [
                    new(owner.Username, owner.Id, ownerRole.Code, owner.IsActive),
                    new(systemAdmin.Username, systemAdmin.Id, systemAdminRole.Code, systemAdmin.IsActive),
                    new(branchAdmin01.Username, branchAdmin01.Id, branchAdminRole.Code, branchAdmin01.IsActive),
                    new(branchAdmin02.Username, branchAdmin02.Id, branchAdminRole.Code, branchAdmin02.IsActive),
                    new(branchAdmin03.Username, branchAdmin03.Id, branchAdminRole.Code, branchAdmin03.IsActive),
                    new(employeeA.Username, employeeA.Id, employeeRole.Code, employeeA.IsActive),
                    new(employeeB.Username, employeeB.Id, employeeRole.Code, employeeB.IsActive),
                    new(lockedEmployee.Username, lockedEmployee.Id, employeeRole.Code, lockedEmployee.IsActive),
                    new(unassignedEmployee.Username, unassignedEmployee.Id, employeeRole.Code, unassignedEmployee.IsActive)
                ],
                [
                    new(branch01.Id, branch01.Code),
                    new(branch02.Id, branch02.Code),
                    new(branch03.Id, branch03.Code)
                ]);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            // CommitTransactionAsync tự giải phóng transaction nếu commit phát sinh lỗi.
            // Khi transaction vẫn còn, rollback sẽ đưa các thay đổi chưa commit về trạng thái ban đầu.
            try
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            }
            catch
            {
                // Giữ nguyên exception gốc để không che mất nguyên nhân thất bại.
            }

            throw;
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumPasswordLength)
        {
            throw new ArgumentException(
                $"Mật khẩu demo phải có ít nhất {MinimumPasswordLength} ký tự.",
                nameof(password));
        }
    }

    private async Task<Role> GetRequiredRoleAsync(
        string code,
        CancellationToken cancellationToken)
    {
        Role? role = await _repository.GetRoleByCodeAsync(code, cancellationToken);

        return role ?? throw new InvalidOperationException(
            $"Không tìm thấy role '{code}'. Hãy chạy database/002-reference-data.sql trước.");
    }

    private async Task<Branch> EnsureBranchAsync(
        string code,
        string name,
        string address,
        DateTime now,
        CancellationToken cancellationToken)
    {
        Branch? existing = await _repository.GetBranchByCodeAsync(code, cancellationToken);

        if (existing is not null)
        {
            if (existing.Deleted ||
                !existing.IsActive ||
                existing.Name != name ||
                existing.Address != address)
            {
                throw new InvalidOperationException(
                    $"Chi nhánh '{code}' đã tồn tại nhưng không khớp dữ liệu demo. Seeder không tự ghi đè.");
            }

            return existing;
        }

        Branch branch = new()
        {
            Code = code,
            Name = name,
            Address = address,
            IsActive = true,
            CreatedAt = now,
            Deleted = false
        };

        _repository.AddBranch(branch);
        return branch;
    }

    private async Task<AppUser> EnsureUserAsync(
        string username,
        string fullName,
        Role role,
        bool isActive,
        string password,
        DateTime now,
        CancellationToken cancellationToken)
    {
        AppUser? existing = await _repository.GetUserByUsernameAsync(username, cancellationToken);

        if (existing is not null)
        {
            if (existing.Deleted ||
                existing.Role.Deleted ||
                existing.Role.Code != role.Code ||
                existing.IsActive != isActive)
            {
                throw new InvalidOperationException(
                    $"Tài khoản '{username}' đang có role/trạng thái khác cấu hình demo. Seeder không tự ghi đè.");
            }

            // Không thay đổi PasswordHash của tài khoản đã tồn tại khi chạy lại.
            return existing;
        }

        AppUser user = new()
        {
            RoleId = role.Id,
            Username = username,
            FullName = fullName,
            IsActive = isActive,
            CreatedAt = now,
            Deleted = false
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _repository.AddUser(user);
        return user;
    }

    private async Task<EmployeeProfile> EnsureEmployeeProfileAsync(
        AppUser user,
        string employeeCode,
        DateTime now,
        CancellationToken cancellationToken)
    {
        EmployeeProfile? existing = await _repository.GetEmployeeProfileByUserIdAsync(
            user.Id,
            cancellationToken);

        if (existing is not null)
        {
            if (existing.Deleted || existing.EmployeeCode != employeeCode)
            {
                throw new InvalidOperationException(
                    $"Hồ sơ nhân viên của '{user.Username}' không khớp cấu hình demo. Seeder không tự ghi đè.");
            }

            return existing;
        }

        EmployeeProfile profile = new()
        {
            UserId = user.Id,
            EmployeeCode = employeeCode,
            HireDate = DateOnly.FromDateTime(now),
            Position = "Demo Employee",
            BaseSalary = 0m,
            CreatedAt = now,
            Deleted = false
        };

        _repository.AddEmployeeProfile(profile);
        return profile;
    }

    private async Task EnsureAdminAssignmentAsync(
        Branch branch,
        AppUser branchAdmin,
        AppUser assignedBy,
        DateTime now,
        CancellationToken cancellationToken)
    {
        AdminBranchAssignment? existing = await _repository.GetOpenAdminBranchAssignmentByBranchIdAsync(
            branch.Id,
            cancellationToken);

        if (existing is not null)
        {
            if (existing.AdminUserId != branchAdmin.Id ||
                existing.AssignedByUserId != assignedBy.Id ||
                existing.EffectiveFrom > now)
            {
                throw new InvalidOperationException(
                    $"Chi nhánh '{branch.Code}' đã có phân công quản trị khác. Seeder không tự thay đổi phân công.");
            }

            return;
        }

        _repository.AddAdminBranchAssignment(new AdminBranchAssignment
        {
            AdminUserId = branchAdmin.Id,
            BranchId = branch.Id,
            EffectiveFrom = now,
            AssignedByUserId = assignedBy.Id,
            CreatedAt = now
        });
    }

    private async Task EnsureEmployeeAssignmentAsync(
        EmployeeProfile profile,
        Branch branch,
        AppUser assignedBy,
        DateTime now,
        CancellationToken cancellationToken)
    {
        EmployeeBranchAssignment? existing = await _repository.GetOpenEmployeeBranchAssignmentByProfileIdAsync(
            profile.Id,
            cancellationToken);

        if (existing is not null)
        {
            if (existing.BranchId != branch.Id ||
                existing.AssignedByUserId != assignedBy.Id ||
                existing.EffectiveFrom > now)
            {
                throw new InvalidOperationException(
                    $"Nhân viên '{profile.EmployeeCode}' đang có phân công khác. Seeder không tự thay đổi phân công.");
            }

            return;
        }

        _repository.AddEmployeeBranchAssignment(new EmployeeBranchAssignment
        {
            EmployeeProfileId = profile.Id,
            BranchId = branch.Id,
            EffectiveFrom = now,
            AssignedByUserId = assignedBy.Id,
            CreatedAt = now
        });
    }
}
