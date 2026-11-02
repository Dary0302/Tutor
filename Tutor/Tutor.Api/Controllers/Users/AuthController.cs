using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Auth;

namespace Tutor.Controllers.Users;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [HttpPost("~/auth/register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var created = await auth.RegisterAsync(request.Email, request.Password, request.Role, cancellationToken);
        return created
            ? Reply(StatusCodes.Status201Created, "User created")
            : Reply(StatusCodes.Status409Conflict, "Email already in use");
    }

    [AllowAnonymous]
    [HttpPost("log-in")]
    [HttpPost("~/auth/authentication")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var session = await auth.LoginAsync(request.Email, request.Password, cancellationToken);
        return session is null
            ? Reply(StatusCodes.Status401Unauthorized, "Invalid email or password")
            : Reply(StatusCodes.Status200OK, "User authenticated", session);
    }

    [AllowAnonymous]
    [HttpPut("refresh")]
    [HttpPost("~/auth/refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var session = await auth.RefreshAsync(request.RefreshToken, cancellationToken);
        return session is null
            ? Reply(StatusCodes.Status401Unauthorized, "Invalid or expired refresh token")
            : Reply(StatusCodes.Status200OK, "Token refreshed", session);
    }

    [Authorize]
    [HttpGet("get-me")]
    public async Task<ActionResult<UserProfile>> GetMe(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id))
            return Unauthorized();

        var profile = await auth.GetProfileAsync(id, cancellationToken);
        return profile is null ? Unauthorized() : Ok(profile);
    }

    private ObjectResult Reply(int status, string message, AuthSession? data = null) =>
        StatusCode(status, new AuthResponse(message, status, data));
}
