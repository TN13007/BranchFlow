using System;
using System.Collections.Generic;
using BranchFlow.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Data;

public partial class BranchFlowDbContext : DbContext
{
    public BranchFlowDbContext(DbContextOptions<BranchFlowDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminBranchAssignment> AdminBranchAssignments { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<AuthSession> AuthSessions { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<BranchIngredient> BranchIngredients { get; set; }

    public virtual DbSet<BranchProduct> BranchProducts { get; set; }

    public virtual DbSet<BranchTopping> BranchToppings { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<EmployeeBranchAssignment> EmployeeBranchAssignments { get; set; }

    public virtual DbSet<EmployeeProfile> EmployeeProfiles { get; set; }

    public virtual DbSet<EmployeeStockPermission> EmployeeStockPermissions { get; set; }

    public virtual DbSet<EmployeeTransferRequest> EmployeeTransferRequests { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<IngredientUnit> IngredientUnits { get; set; }

    public virtual DbSet<NoteGroup> NoteGroups { get; set; }

    public virtual DbSet<NoteOption> NoteOptions { get; set; }

    public virtual DbSet<OrderAdjustment> OrderAdjustments { get; set; }

    public virtual DbSet<OrderDailyCounter> OrderDailyCounters { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderItemNote> OrderItemNotes { get; set; }

    public virtual DbSet<OrderItemTopping> OrderItemToppings { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalesOrder> SalesOrders { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<StockTransaction> StockTransactions { get; set; }

    public virtual DbSet<StockTransactionDetail> StockTransactionDetails { get; set; }

    public virtual DbSet<Stocktake> Stocktakes { get; set; }

    public virtual DbSet<StocktakeItem> StocktakeItems { get; set; }

    public virtual DbSet<Topping> Toppings { get; set; }

    public virtual DbSet<ToppingGroup> ToppingGroups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_100_CI_AI");

        modelBuilder.Entity<AdminBranchAssignment>(entity =>
        {
            entity.ToTable("AdminBranchAssignment");

            entity.HasIndex(e => new { e.AdminUserId, e.EffectiveFrom, e.EffectiveTo }, "IX_AdminBranchAssignment_AdminUserId_EffectiveFrom_EffectiveTo");

            entity.HasIndex(e => e.AssignedByUserId, "IX_AdminBranchAssignment_AssignedByUserId_FK");

            entity.HasIndex(e => e.BranchId, "IX_AdminBranchAssignment_BranchId_FK");

            entity.HasIndex(e => e.BranchId, "UX_AdminBranchAssignment_OpenBranch")
                .IsUnique()
                .HasFilter("([EffectiveTo] IS NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_AdminBranchAssignment_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.AdminUser).WithMany(p => p.AdminBranchAssignmentAdminUsers)
                .HasForeignKey(d => d.AdminUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AdminBranchAssignment_AdminUserId");

            entity.HasOne(d => d.AssignedByUser).WithMany(p => p.AdminBranchAssignmentAssignedByUsers)
                .HasForeignKey(d => d.AssignedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AdminBranchAssignment_AssignedByUserId");

            entity.HasOne(d => d.Branch).WithOne(p => p.AdminBranchAssignment)
                .HasForeignKey<AdminBranchAssignment>(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AdminBranchAssignment_BranchId");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUser");

            entity.HasIndex(e => e.RoleId, "IX_AppUser_RoleId_FK");

            entity.HasIndex(e => e.Email, "UX_AppUser_Email")
                .IsUnique()
                .HasFilter("([Deleted]=(0) AND [Email] IS NOT NULL)");

            entity.HasIndex(e => e.Username, "UX_AppUser_Username")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_AppUser_CreatedAt");
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_AppUser_IsActive");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppUser_RoleId");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog");

            entity.HasIndex(e => new { e.BranchId, e.CreatedAt }, "IX_AuditLog_BranchId_CreatedAt");

            entity.HasIndex(e => new { e.EntityName, e.EntityId, e.CreatedAt }, "IX_AuditLog_EntityName_EntityId_CreatedAt");

            entity.HasIndex(e => e.PerformedByUserId, "IX_AuditLog_PerformedByUserId_FK");

            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_AuditLog_CreatedAt");
            entity.Property(e => e.EntityName).HasMaxLength(100);

            entity.HasOne(d => d.Branch).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_AuditLog_BranchId");

            entity.HasOne(d => d.PerformedByUser).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.PerformedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLog_PerformedByUserId");
        });

        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("AuthSession");

            entity.HasIndex(e => new { e.UserId, e.ExpiresAt }, "IX_AuthSession_UserId_ExpiresAt");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_AuthSession_CreatedAt");

            entity.HasOne(d => d.User).WithMany(p => p.AuthSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuthSession_UserId");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branch");

            entity.HasIndex(e => e.Code, "UX_Branch_Code")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Branch_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Branch_IsActive");
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<BranchIngredient>(entity =>
        {
            entity.ToTable("BranchIngredient");

            entity.HasIndex(e => e.IngredientId, "IX_BranchIngredient_IngredientId_FK");

            entity.HasIndex(e => new { e.BranchId, e.IngredientId }, "UX_BranchIngredient_BranchId_IngredientId").IsUnique();

            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.WarningThreshold).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchIngredients)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BranchIngredient_BranchId");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.BranchIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BranchIngredient_IngredientId");
        });

        modelBuilder.Entity<BranchProduct>(entity =>
        {
            entity.ToTable("BranchProduct");

            entity.HasIndex(e => e.ProductId, "IX_BranchProduct_ProductId_FK");

            entity.HasIndex(e => new { e.BranchId, e.ProductId }, "UX_BranchProduct_BranchId_ProductId").IsUnique();

            entity.Property(e => e.IsAvailable).HasDefaultValue(true, "DF_BranchProduct_IsAvailable");

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchProducts)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BranchProduct_BranchId");

            entity.HasOne(d => d.Product).WithMany(p => p.BranchProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BranchProduct_ProductId");
        });

        modelBuilder.Entity<BranchTopping>(entity =>
        {
            entity.ToTable("BranchTopping");

            entity.HasIndex(e => e.ToppingId, "IX_BranchTopping_ToppingId_FK");

            entity.HasIndex(e => new { e.BranchId, e.ToppingId }, "UX_BranchTopping_BranchId_ToppingId").IsUnique();

            entity.Property(e => e.IsAvailable).HasDefaultValue(true, "DF_BranchTopping_IsAvailable");

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchToppings)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BranchTopping_BranchId");

            entity.HasOne(d => d.Topping).WithMany(p => p.BranchToppings)
                .HasForeignKey(d => d.ToppingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BranchTopping_ToppingId");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.HasIndex(e => e.Name, "UX_Category_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Category_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Category_IsActive");
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<EmployeeBranchAssignment>(entity =>
        {
            entity.ToTable("EmployeeBranchAssignment");

            entity.HasIndex(e => e.AssignedByUserId, "IX_EmployeeBranchAssignment_AssignedByUserId_FK");

            entity.HasIndex(e => new { e.BranchId, e.EffectiveFrom, e.EffectiveTo }, "IX_EmployeeBranchAssignment_BranchId_EffectiveFrom_EffectiveTo");

            entity.HasIndex(e => e.EmployeeProfileId, "IX_EmployeeBranchAssignment_EmployeeProfileId_FK");

            entity.HasIndex(e => e.TransferRequestId, "IX_EmployeeBranchAssignment_TransferRequestId_FK");

            entity.HasIndex(e => e.EmployeeProfileId, "UX_EmployeeBranchAssignment_OpenEmployee")
                .IsUnique()
                .HasFilter("([EffectiveTo] IS NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_EmployeeBranchAssignment_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.AssignedByUser).WithMany(p => p.EmployeeBranchAssignments)
                .HasForeignKey(d => d.AssignedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeBranchAssignment_AssignedByUserId");

            entity.HasOne(d => d.Branch).WithMany(p => p.EmployeeBranchAssignments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeBranchAssignment_BranchId");

            entity.HasOne(d => d.EmployeeProfile).WithOne(p => p.EmployeeBranchAssignment)
                .HasForeignKey<EmployeeBranchAssignment>(d => d.EmployeeProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeBranchAssignment_EmployeeProfileId");

            entity.HasOne(d => d.TransferRequest).WithMany(p => p.EmployeeBranchAssignments)
                .HasForeignKey(d => d.TransferRequestId)
                .HasConstraintName("FK_EmployeeBranchAssignment_TransferRequestId");
        });

        modelBuilder.Entity<EmployeeProfile>(entity =>
        {
            entity.ToTable("EmployeeProfile");

            entity.HasIndex(e => e.EmployeeCode, "UX_EmployeeProfile_EmployeeCode")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.HasIndex(e => e.UserId, "UX_EmployeeProfile_UserId").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AvatarUrl).HasMaxLength(1000);
            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_EmployeeProfile_CreatedAt");
            entity.Property(e => e.EmployeeCode).HasMaxLength(30);
            entity.Property(e => e.Position).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.EmployeeProfile)
                .HasForeignKey<EmployeeProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeProfile_UserId");
        });

        modelBuilder.Entity<EmployeeStockPermission>(entity =>
        {
            entity.ToTable("EmployeeStockPermission");

            entity.HasIndex(e => e.BranchId, "IX_EmployeeStockPermission_BranchId_FK");

            entity.HasIndex(e => new { e.EmployeeProfileId, e.BranchId }, "UX_EmployeeStockPermission_EmployeeProfileId_BranchId").IsUnique();

            entity.HasOne(d => d.Branch).WithMany(p => p.EmployeeStockPermissions)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeStockPermission_BranchId");

            entity.HasOne(d => d.EmployeeProfile).WithMany(p => p.EmployeeStockPermissions)
                .HasForeignKey(d => d.EmployeeProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeStockPermission_EmployeeProfileId");
        });

        modelBuilder.Entity<EmployeeTransferRequest>(entity =>
        {
            entity.ToTable("EmployeeTransferRequest");

            entity.HasIndex(e => new { e.EmployeeProfileId, e.Status }, "IX_EmployeeTransferRequest_EmployeeProfileId_Status");

            entity.HasIndex(e => e.FromBranchId, "IX_EmployeeTransferRequest_FromBranchId_FK");

            entity.HasIndex(e => e.RequestedByUserId, "IX_EmployeeTransferRequest_RequestedByUserId_FK");

            entity.HasIndex(e => e.ReviewedByUserId, "IX_EmployeeTransferRequest_ReviewedByUserId_FK");

            entity.HasIndex(e => e.ToBranchId, "IX_EmployeeTransferRequest_ToBranchId_FK");

            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_EmployeeTransferRequest_RequestedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING", "DF_EmployeeTransferRequest_Status");

            entity.HasOne(d => d.EmployeeProfile).WithMany(p => p.EmployeeTransferRequests)
                .HasForeignKey(d => d.EmployeeProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeTransferRequest_EmployeeProfileId");

            entity.HasOne(d => d.FromBranch).WithMany(p => p.EmployeeTransferRequestFromBranches)
                .HasForeignKey(d => d.FromBranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeTransferRequest_FromBranchId");

            entity.HasOne(d => d.RequestedByUser).WithMany(p => p.EmployeeTransferRequestRequestedByUsers)
                .HasForeignKey(d => d.RequestedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeTransferRequest_RequestedByUserId");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.EmployeeTransferRequestReviewedByUsers)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK_EmployeeTransferRequest_ReviewedByUserId");

            entity.HasOne(d => d.ToBranch).WithMany(p => p.EmployeeTransferRequestToBranches)
                .HasForeignKey(d => d.ToBranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeTransferRequest_ToBranchId");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.ToTable("Ingredient");

            entity.HasIndex(e => e.UnitId, "IX_Ingredient_UnitId_FK");

            entity.HasIndex(e => e.Name, "UX_Ingredient_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Ingredient_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Ingredient_IsActive");
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Unit).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingredient_UnitId");
        });

        modelBuilder.Entity<IngredientUnit>(entity =>
        {
            entity.ToTable("IngredientUnit");

            entity.HasIndex(e => e.Name, "UX_IngredientUnit_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_IngredientUnit_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_IngredientUnit_IsActive");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<NoteGroup>(entity =>
        {
            entity.ToTable("NoteGroup");

            entity.HasIndex(e => e.Name, "UX_NoteGroup_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_NoteGroup_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_NoteGroup_IsActive");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<NoteOption>(entity =>
        {
            entity.ToTable("NoteOption");

            entity.HasIndex(e => e.NoteGroupId, "IX_NoteOption_NoteGroupId_FK");

            entity.HasIndex(e => new { e.NoteGroupId, e.Name }, "UX_NoteOption_NoteGroupId_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_NoteOption_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_NoteOption_IsActive");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.NoteGroup).WithMany(p => p.NoteOptions)
                .HasForeignKey(d => d.NoteGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NoteOption_NoteGroupId");
        });

        modelBuilder.Entity<OrderAdjustment>(entity =>
        {
            entity.ToTable("OrderAdjustment");

            entity.HasIndex(e => e.AdjustedByUserId, "IX_OrderAdjustment_AdjustedByUserId_FK");

            entity.HasIndex(e => new { e.SalesOrderId, e.CreatedAt }, "IX_OrderAdjustment_SalesOrderId_CreatedAt");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_OrderAdjustment_CreatedAt");
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasOne(d => d.AdjustedByUser).WithMany(p => p.OrderAdjustments)
                .HasForeignKey(d => d.AdjustedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderAdjustment_AdjustedByUserId");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.OrderAdjustments)
                .HasForeignKey(d => d.SalesOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderAdjustment_SalesOrderId");
        });

        modelBuilder.Entity<OrderDailyCounter>(entity =>
        {
            entity.HasKey(e => new { e.BranchId, e.BusinessDate });

            entity.ToTable("OrderDailyCounter");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_OrderDailyCounter_UpdatedAt");

            entity.HasOne(d => d.Branch).WithMany(p => p.OrderDailyCounters)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDailyCounter_BranchId");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItem");

            entity.HasIndex(e => e.ProductSizeId, "IX_OrderItem_ProductSizeId_FK");

            entity.HasIndex(e => e.SalesOrderId, "IX_OrderItem_SalesOrderId_FK");

            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.ProductNameSnapshot).HasMaxLength(150);
            entity.Property(e => e.SizeNameSnapshot).HasMaxLength(50);
            entity.Property(e => e.SubtotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPriceSnapshot).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ProductSize).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductSizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItem_ProductSizeId");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.SalesOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItem_SalesOrderId");
        });

        modelBuilder.Entity<OrderItemNote>(entity =>
        {
            entity.ToTable("OrderItemNote");

            entity.HasIndex(e => e.NoteOptionId, "IX_OrderItemNote_NoteOptionId_FK");

            entity.HasIndex(e => new { e.OrderItemId, e.NoteOptionId }, "UX_OrderItemNote_OrderItemId_NoteOptionId").IsUnique();

            entity.Property(e => e.NoteNameSnapshot).HasMaxLength(100);

            entity.HasOne(d => d.NoteOption).WithMany(p => p.OrderItemNotes)
                .HasForeignKey(d => d.NoteOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItemNote_NoteOptionId");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.OrderItemNotes)
                .HasForeignKey(d => d.OrderItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItemNote_OrderItemId");
        });

        modelBuilder.Entity<OrderItemTopping>(entity =>
        {
            entity.ToTable("OrderItemTopping");

            entity.HasIndex(e => e.ToppingId, "IX_OrderItemTopping_ToppingId_FK");

            entity.HasIndex(e => new { e.OrderItemId, e.ToppingId }, "UX_OrderItemTopping_OrderItemId_ToppingId").IsUnique();

            entity.Property(e => e.ToppingNameSnapshot).HasMaxLength(150);
            entity.Property(e => e.UnitPriceSnapshot).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.OrderItemToppings)
                .HasForeignKey(d => d.OrderItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItemTopping_OrderItemId");

            entity.HasOne(d => d.Topping).WithMany(p => p.OrderItemToppings)
                .HasForeignKey(d => d.ToppingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItemTopping_ToppingId");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.ToTable("Payroll");

            entity.HasIndex(e => e.BranchId, "IX_Payroll_BranchId_FK");

            entity.HasIndex(e => e.CreatedByUserId, "IX_Payroll_CreatedByUserId_FK");

            entity.HasIndex(e => e.UpdatedByUserId, "IX_Payroll_UpdatedByUserId_FK");

            entity.HasIndex(e => new { e.EmployeeProfileId, e.Year, e.Month }, "UX_Payroll_EmployeeProfileId_Year_Month").IsUnique();

            entity.Property(e => e.AllowanceAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BaseSalarySnapshot).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BonusAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Payroll_CreatedAt");
            entity.Property(e => e.DeductionAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TotalSalary)
                .HasComputedColumnSql("(CONVERT([decimal](18,2),(([BaseSalarySnapshot]+[AllowanceAmount])+[BonusAmount])-[DeductionAmount]))", true)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Payroll_UpdatedAt");

            entity.HasOne(d => d.Branch).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payroll_BranchId");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PayrollCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payroll_CreatedByUserId");

            entity.HasOne(d => d.EmployeeProfile).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.EmployeeProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payroll_EmployeeProfileId");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PayrollUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payroll_UpdatedByUserId");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.HasIndex(e => e.CategoryId, "IX_Product_CategoryId_FK");

            entity.HasIndex(e => e.Name, "UX_Product_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Product_CreatedAt");
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Product_IsActive");
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_CategoryId");
        });

        modelBuilder.Entity<ProductSize>(entity =>
        {
            entity.ToTable("ProductSize");

            entity.HasIndex(e => e.ProductId, "IX_ProductSize_ProductId_FK");

            entity.HasIndex(e => e.SizeId, "IX_ProductSize_SizeId_FK");

            entity.HasIndex(e => new { e.ProductId, e.SizeId }, "UX_ProductSize_ProductId_SizeId")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_ProductSize_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ProductSize_IsActive");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductSizes)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductSize_ProductId");

            entity.HasOne(d => d.Size).WithMany(p => p.ProductSizes)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductSize_SizeId");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshToken");

            entity.HasIndex(e => e.SessionId, "IX_RefreshToken_SessionId");

            entity.HasIndex(e => e.TokenHash, "UX_RefreshToken_TokenHash").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_RefreshToken_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.Session).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshToken_SessionId");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.HasIndex(e => e.Code, "UX_Role_Code")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.Code).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Role_CreatedAt");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.ToTable("SalesOrder");

            entity.HasIndex(e => new { e.BranchId, e.BusinessDate, e.Status }, "IX_SalesOrder_BranchId_BusinessDate_Status");

            entity.HasIndex(e => new { e.CreatedByUserId, e.CreatedAt }, "IX_SalesOrder_CreatedByUserId_CreatedAt");

            entity.HasIndex(e => e.ReportedByUserId, "IX_SalesOrder_ReportedByUserId_FK");

            entity.HasIndex(e => new { e.BranchId, e.BusinessDate, e.DailySequence }, "UX_SalesOrder_BranchId_BusinessDate_DailySequence").IsUnique();

            entity.HasIndex(e => e.Code, "UX_SalesOrder_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_SalesOrder_CreatedAt");
            entity.Property(e => e.ReportReason).HasMaxLength(500);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("COMPLETED", "DF_SalesOrder_Status");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Branch).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesOrder_BranchId");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.SalesOrderCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesOrder_CreatedByUserId");

            entity.HasOne(d => d.ReportedByUser).WithMany(p => p.SalesOrderReportedByUsers)
                .HasForeignKey(d => d.ReportedByUserId)
                .HasConstraintName("FK_SalesOrder_ReportedByUserId");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.ToTable("Size");

            entity.HasIndex(e => e.Name, "UX_Size_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Size_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Size_IsActive");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.ToTable("StockTransaction");

            entity.HasIndex(e => new { e.BranchId, e.CreatedAt }, "IX_StockTransaction_BranchId_CreatedAt");

            entity.HasIndex(e => e.OriginalTransactionId, "IX_StockTransaction_OriginalTransactionId_FK");

            entity.HasIndex(e => e.PerformedByUserId, "IX_StockTransaction_PerformedByUserId_FK");

            entity.HasIndex(e => e.OriginalTransactionId, "UX_StockTransaction_OriginalTransactionId")
                .IsUnique()
                .HasFilter("([OriginalTransactionId] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_StockTransaction_CreatedAt");
            entity.Property(e => e.Type).HasMaxLength(30);

            entity.HasOne(d => d.Branch).WithMany(p => p.StockTransactions)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransaction_BranchId");

            entity.HasOne(d => d.OriginalTransaction).WithOne(p => p.InverseOriginalTransaction)
                .HasForeignKey<StockTransaction>(d => d.OriginalTransactionId)
                .HasConstraintName("FK_StockTransaction_OriginalTransactionId");

            entity.HasOne(d => d.PerformedByUser).WithMany(p => p.StockTransactions)
                .HasForeignKey(d => d.PerformedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransaction_PerformedByUserId");
        });

        modelBuilder.Entity<StockTransactionDetail>(entity =>
        {
            entity.ToTable("StockTransactionDetail");

            entity.HasIndex(e => e.IngredientId, "IX_StockTransactionDetail_IngredientId_FK");

            entity.HasIndex(e => new { e.StockTransactionId, e.IngredientId }, "UX_StockTransactionDetail_StockTransactionId_IngredientId").IsUnique();

            entity.Property(e => e.QuantityAfter).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.QuantityBefore).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.QuantityChange).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.StockTransactionDetails)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransactionDetail_IngredientId");

            entity.HasOne(d => d.StockTransaction).WithMany(p => p.StockTransactionDetails)
                .HasForeignKey(d => d.StockTransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransactionDetail_StockTransactionId");
        });

        modelBuilder.Entity<Stocktake>(entity =>
        {
            entity.ToTable("Stocktake");

            entity.HasIndex(e => e.AdjustmentTransactionId, "IX_Stocktake_AdjustmentTransactionId_FK");

            entity.HasIndex(e => new { e.BranchId, e.CreatedAt }, "IX_Stocktake_BranchId_CreatedAt");

            entity.HasIndex(e => e.CheckedByUserId, "IX_Stocktake_CheckedByUserId_FK");

            entity.HasIndex(e => e.ReviewedByUserId, "IX_Stocktake_ReviewedByUserId_FK");

            entity.HasIndex(e => e.AdjustmentTransactionId, "UX_Stocktake_AdjustmentTransactionId")
                .IsUnique()
                .HasFilter("([AdjustmentTransactionId] IS NOT NULL)");

            entity.HasIndex(e => e.Code, "UX_Stocktake_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Stocktake_CreatedAt");
            entity.Property(e => e.ReviewReason).HasMaxLength(500);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("DRAFT", "DF_Stocktake_Status");

            entity.HasOne(d => d.AdjustmentTransaction).WithOne(p => p.Stocktake)
                .HasForeignKey<Stocktake>(d => d.AdjustmentTransactionId)
                .HasConstraintName("FK_Stocktake_AdjustmentTransactionId");

            entity.HasOne(d => d.Branch).WithMany(p => p.Stocktakes)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stocktake_BranchId");

            entity.HasOne(d => d.CheckedByUser).WithMany(p => p.StocktakeCheckedByUsers)
                .HasForeignKey(d => d.CheckedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stocktake_CheckedByUserId");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.StocktakeReviewedByUsers)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK_Stocktake_ReviewedByUserId");
        });

        modelBuilder.Entity<StocktakeItem>(entity =>
        {
            entity.ToTable("StocktakeItem");

            entity.HasIndex(e => e.BranchIngredientId, "IX_StocktakeItem_BranchIngredientId_FK");

            entity.HasIndex(e => new { e.StocktakeId, e.BranchIngredientId }, "UX_StocktakeItem_StocktakeId_BranchIngredientId").IsUnique();

            entity.Property(e => e.ActualQuantity).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Difference)
                .HasComputedColumnSql("(CONVERT([decimal](18,3),[ActualQuantity]-[SystemQuantity]))", true)
                .HasColumnType("decimal(18, 3)");
            entity.Property(e => e.InventoryRowVersionSnapshot)
                .HasMaxLength(8)
                .IsFixedLength();
            entity.Property(e => e.SystemQuantity).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.BranchIngredient).WithMany(p => p.StocktakeItems)
                .HasForeignKey(d => d.BranchIngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StocktakeItem_BranchIngredientId");

            entity.HasOne(d => d.Stocktake).WithMany(p => p.StocktakeItems)
                .HasForeignKey(d => d.StocktakeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StocktakeItem_StocktakeId");
        });

        modelBuilder.Entity<Topping>(entity =>
        {
            entity.ToTable("Topping");

            entity.HasIndex(e => e.ToppingGroupId, "IX_Topping_ToppingGroupId_FK");

            entity.HasIndex(e => e.Name, "UX_Topping_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Topping_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Topping_IsActive");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ToppingGroup).WithMany(p => p.Toppings)
                .HasForeignKey(d => d.ToppingGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Topping_ToppingGroupId");
        });

        modelBuilder.Entity<ToppingGroup>(entity =>
        {
            entity.ToTable("ToppingGroup");

            entity.HasIndex(e => e.Name, "UX_ToppingGroup_Name")
                .IsUnique()
                .HasFilter("([Deleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_ToppingGroup_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ToppingGroup_IsActive");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
