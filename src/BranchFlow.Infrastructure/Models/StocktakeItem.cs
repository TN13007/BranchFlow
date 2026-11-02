using System;
using System.Collections.Generic;

namespace BranchFlow.Infrastructure.Models;

public partial class StocktakeItem
{
    public int Id { get; set; }

    public int StocktakeId { get; set; }

    public int BranchIngredientId { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal ActualQuantity { get; set; }

    public byte[] InventoryRowVersionSnapshot { get; set; } = null!;

    public decimal Difference { get; set; }

    public virtual BranchIngredient BranchIngredient { get; set; } = null!;

    public virtual Stocktake Stocktake { get; set; } = null!;
}
