using Auth.Domain.Entities;

namespace Auth.Application.DTOs;

public class UserResponseDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public Roles Role { get; set; }
    public string? Token { get; set; }
    public Guid? RefreshToken { get; set; }
}