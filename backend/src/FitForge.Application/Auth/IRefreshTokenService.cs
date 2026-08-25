namespace FitForge.Application.Auth;

public record RefreshTokenRotationResult(bool Succeeded, string? UserId, string? NewToken);

public interface IRefreshTokenService
{
    Task<string> CreateAsync(string userId, CancellationToken cancellationToken);

    Task<RefreshTokenRotationResult> RotateAsync(string token, CancellationToken cancellationToken);
}
