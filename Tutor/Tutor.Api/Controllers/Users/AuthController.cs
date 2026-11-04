using Core.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Auth;

namespace Tutor.Controllers.Users;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : TutorControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [HttpPost("~/auth/register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var created = await auth.RegisterAsync(request.Email, request.Password, request.Role, cancellationToken);
        return Reply(created, StatusCodes.Status201Created, "User created");
    }

    [AllowAnonymous]
    [HttpPost("log-in")]
    [HttpPost("~/auth/authentication")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var session = await auth.LoginAsync(request.Email, request.Password, cancellationToken);
        return Reply(session, StatusCodes.Status200OK, "User authenticated", session.ValueOrDefault);
    }

    [AllowAnonymous]
    [HttpPut("refresh")]
    [HttpPost("~/auth/refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var session = await auth.RefreshAsync(request.RefreshToken, cancellationToken);
        return Reply(session, StatusCodes.Status200OK, "Token refreshed", session.ValueOrDefault);
    }

    [Authorize]
    [HttpGet("get-me")]
    public async Task<ActionResult<UserProfile>> GetMe(CancellationToken cancellationToken)
    {
        var profile = await auth.GetProfileAsync(GetIdAuthorizedUser(), cancellationToken);
        return profile.ToActionResult();
    }

    private ObjectResult Reply(IResultBase result, int status, string message, AuthSession? data = null)
    {
        if (result.IsFailed)
        {
            var error = result.GetAppError();
            status = error.Status.GetHttpResponseCode();
            message = error.Message;
        }
        return StatusCode(status, new AuthResponse(message, status, data));
    }
}
