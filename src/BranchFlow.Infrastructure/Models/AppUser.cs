using System;
using System.Collections.Generic;

namespace BranchFlow.Infrastructure.Models;

public partial class AppUser
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<AdminBranchAssignment> AdminBranchAssignmentAdminUsers { get; set; } = new List<AdminBranchAssignment>();

    public virtual ICollection<AdminBranchAssignment> AdminBranchAssignmentAssignedByUsers { get; set; } = new List<AdminBranchAssignment>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<AuthSession> AuthSessions { get; set; } = new List<AuthSession>();

    public virtual ICollection<EmployeeBranchAssignment> EmployeeBranchAssignments { get; set; } = new List<EmployeeBranchAssignment>();

    public virtual EmployeeProfile? EmployeeProfile { get; set; }

    public virtual ICollection<EmployeeTransferRequest> EmployeeTransferRequestRequestedByUsers { get; set; } = new List<EmployeeTransferRequest>();

    public virtual ICollection<EmployeeTransferRequest> EmployeeTransferRequestReviewedByUsers { get; set; } = new List<EmployeeTransferRequest>();

    public virtual ICollection<OrderAdjustment> OrderAdjustments { get; set; } = new List<OrderAdjustment>();

    public virtual ICollection<Payroll> PayrollCreatedByUsers { get; set; } = new List<Payroll>();

    public virtual ICollection<Payroll> PayrollUpdatedByUsers { get; set; } = new List<Payroll>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SalesOrder> SalesOrderCreatedByUsers { get; set; } = new List<SalesOrder>();

    public virtual ICollection<SalesOrder> SalesOrderReportedByUsers { get; set; } = new List<SalesOrder>();

    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    public virtual ICollection<Stocktake> StocktakeCheckedByUsers { get; set; } = new List<Stocktake>();

    public virtual ICollection<Stocktake> StocktakeReviewedByUsers { get; set; } = new List<Stocktake>();
}
