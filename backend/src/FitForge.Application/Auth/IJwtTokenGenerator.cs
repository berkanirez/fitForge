namespace FitForge.Application.Auth;

public interface IJwtTokenGenerator
{
    LoginResponse CreateToken(string userId, string email, IEnumerable<string> roles);
}
