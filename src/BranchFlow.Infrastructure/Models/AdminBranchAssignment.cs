using System;
using System.Collections.Generic;

namespace BranchFlow.Infrastructure.Models;

public partial class AdminBranchAssignment
{
    public int Id { get; set; }

    public int AdminUserId { get; set; }

    public int BranchId { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public int AssignedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual AppUser AdminUser { get; set; } = null!;

    public virtual AppUser AssignedByUser { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;
}
