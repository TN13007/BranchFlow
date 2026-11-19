using BranchFlow.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Data;

public partial class BranchFlowDbContext
{
    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordResetRequest>(e =>
        {
            e.ToTable("PasswordResetRequest");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique().HasFilter("[ResolvedAt] IS NULL")
                .HasDatabaseName("UX_PasswordResetRequest_PendingUser");
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.ResolvedByUser).WithMany().HasForeignKey(x => x.ResolvedByUserId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
