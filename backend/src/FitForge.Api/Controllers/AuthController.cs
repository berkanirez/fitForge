using System.Security.Claims;
using FitForge.Application.Auth;
using FitForge.Domain.Common;
using FitForge.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitForge.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService,
    IValidator<LoginRequest> loginRequestValidator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        await userManager.AddToRoleAsync(user, Roles.User);

        return Ok(new { user.Id, user.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await loginRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenGenerator.CreateToken(user.Id, user.Email!, roles);
        var refreshToken = await refreshTokenService.CreateAsync(user.Id, cancellationToken);

        return Ok(new LoginResponse(accessToken.AccessToken, accessToken.ExpiresAtUtc, refreshToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var rotation = await refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken);

        if (!rotation.Succeeded || rotation.UserId is null || rotation.NewToken is null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(rotation.UserId);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenGenerator.CreateToken(user.Id, user.Email!, roles);

        return Ok(new LoginResponse(accessToken.AccessToken, accessToken.ExpiresAtUtc, rotation.NewToken));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        return Ok(new { userId, email, roles });
    }
}
