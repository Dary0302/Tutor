using System.Text.Json.Serialization;
using Tutor.Application.Auth;
using Tutor.Domain.Users;

namespace Tutor.Controllers.Users;

public record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(string Email, string Password,
    [property: JsonConverter(typeof(JsonStringEnumConverter<UserRole>))] UserRole Role)
    : LoginRequest(Email, Password);

public sealed record RefreshRequest(Guid RefreshToken);

public sealed record AuthResponse(string Message, int Status, AuthSession? Data);
