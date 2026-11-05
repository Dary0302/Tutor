using System.Security.Claims;
using Core.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Tutor.Domain.Users;

namespace Tutor.Controllers;

public abstract class TutorControllerBase : ControllerBase
{
    private const string UnauthorizedUserMessage = "Пользователь не авторизован.";

    protected Guid GetIdAuthorizedUser()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(claim?.Value, out var userId))
            throw new InvalidOperationException(UnauthorizedUserMessage);

        return userId;
    }

    protected Result<UserRole> GetRoleAuthorizedUser()
    {
        var claim = User.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<UserRole>(claim, out var role) && Enum.IsDefined(role)
            ? Result.Ok(role)
            : Result.Fail(AppError.Unauthorized());
    }
}
