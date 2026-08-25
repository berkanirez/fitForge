namespace FitForge.Application.Auth;

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
