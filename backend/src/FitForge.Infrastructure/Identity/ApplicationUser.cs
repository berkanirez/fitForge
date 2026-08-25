using Microsoft.AspNetCore.Identity;

namespace FitForge.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
