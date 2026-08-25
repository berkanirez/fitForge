using FitForge.Domain.Common;

namespace FitForge.Infrastructure.Identity;

public class RefreshToken : AuditableEntity
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByToken { get; set; }

    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
}
