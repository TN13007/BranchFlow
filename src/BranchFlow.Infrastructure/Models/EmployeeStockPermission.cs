using System;
using System.Collections.Generic;

namespace BranchFlow.Infrastructure.Models;

public partial class EmployeeStockPermission
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }

    public int BranchId { get; set; }

    public bool CanReceive { get; set; }

    public bool CanIssue { get; set; }

    public bool CanStocktake { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual EmployeeProfile EmployeeProfile { get; set; } = null!;
}
