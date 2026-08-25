namespace FitForge.Application.Auth;

public record AccessTokenResult(string AccessToken, DateTime ExpiresAtUtc);
