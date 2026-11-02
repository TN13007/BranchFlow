using System;
using System.Collections.Generic;

namespace BranchFlow.Infrastructure.Models;

public partial class Payroll
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }

    public int BranchId { get; set; }

    public short Year { get; set; }

    public byte Month { get; set; }

    public decimal BaseSalarySnapshot { get; set; }

    public decimal TotalSalary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public decimal AllowanceAmount { get; set; }

    public decimal BonusAmount { get; set; }

    public decimal DeductionAmount { get; set; }

    public int CreatedByUserId { get; set; }

    public int UpdatedByUserId { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual EmployeeProfile EmployeeProfile { get; set; } = null!;

    public virtual AppUser UpdatedByUser { get; set; } = null!;
}
