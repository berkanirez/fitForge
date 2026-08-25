namespace FitForge.Application.Auth;

public interface IJwtTokenGenerator
{
    AccessTokenResult CreateToken(string userId, string email, IEnumerable<string> roles);
}
