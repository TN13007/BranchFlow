using System;
using System.Collections.Generic;

namespace BranchFlow.Infrastructure.Models;

public partial class Stocktake
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public int BranchId { get; set; }

    public int CheckedByUserId { get; set; }

    public int? AdjustmentTransactionId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string Status { get; set; } = null!;

    public int? ReviewedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewReason { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual StockTransaction? AdjustmentTransaction { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual AppUser CheckedByUser { get; set; } = null!;

    public virtual AppUser? ReviewedByUser { get; set; }

    public virtual ICollection<StocktakeItem> StocktakeItems { get; set; } = new List<StocktakeItem>();
}
