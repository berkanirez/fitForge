using System.Security.Cryptography;
using FitForge.Application.Auth;
using FitForge.Infrastructure.Identity;
using FitForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitForge.Infrastructure.Authentication;

public class RefreshTokenService(FitForgeDbContext dbContext) : IRefreshTokenService
{
    private const int ExpiryDays = 7;

    public async Task<string> CreateAsync(string userId, CancellationToken cancellationToken)
    {
        var token = GenerateSecureToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(ExpiryDays)
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<RefreshTokenRotationResult> RotateAsync(string token, CancellationToken cancellationToken)
    {
        var existing = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (existing is null)
        {
            return new RefreshTokenRotationResult(false, null, null);
        }

        var isExpired = existing.ExpiresAtUtc < DateTime.UtcNow;

        if (existing.RevokedAtUtc is not null || isExpired)
        {
            if (existing.RevokedAtUtc is not null)
            {
                // Zaten iptal edilmis bir token tekrar kullanilmaya calisildi - bu bir CALINMA belirtisi
                // olabilir. Guvenlik icin, bu kullanicinin TUM aktif token'larini iptal ediyoruz.
                await RevokeAllActiveTokensAsync(existing.UserId, cancellationToken);
            }

            return new RefreshTokenRotationResult(false, null, null);
        }

        var newToken = GenerateSecureToken();

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByToken = newToken;

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = newToken,
            UserId = existing.UserId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(ExpiryDays)
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenRotationResult(true, existing.UserId, newToken);
    }

    private async Task RevokeAllActiveTokensAsync(string userId, CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes);
    }
}
