namespace BranchFlow.Infrastructure.Models;

public sealed class PasswordResetRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedByUserId { get; set; }
    public AppUser User { get; set; } = null!;
    public AppUser? ResolvedByUser { get; set; }
}
