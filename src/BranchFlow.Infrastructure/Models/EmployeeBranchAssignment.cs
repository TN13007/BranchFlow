using System;
using System.Collections.Generic;

namespace BranchFlow.Infrastructure.Models;

public partial class EmployeeBranchAssignment
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }

    public int BranchId { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public int AssignedByUserId { get; set; }

    public int? TransferRequestId { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual AppUser AssignedByUser { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;

    public virtual EmployeeProfile EmployeeProfile { get; set; } = null!;

    public virtual EmployeeTransferRequest? TransferRequest { get; set; }
}
