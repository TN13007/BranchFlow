using System;
using System.Collections.Generic;

namespace BranchFlow.Infrastructure.Models;

public partial class EmployeeTransferRequest
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }

    public int FromBranchId { get; set; }

    public int ToBranchId { get; set; }

    public int RequestedByUserId { get; set; }

    public int? ReviewedByUserId { get; set; }

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<EmployeeBranchAssignment> EmployeeBranchAssignments { get; set; } = new List<EmployeeBranchAssignment>();

    public virtual EmployeeProfile EmployeeProfile { get; set; } = null!;

    public virtual Branch FromBranch { get; set; } = null!;

    public virtual AppUser RequestedByUser { get; set; } = null!;

    public virtual AppUser? ReviewedByUser { get; set; }

    public virtual Branch ToBranch { get; set; } = null!;
}
