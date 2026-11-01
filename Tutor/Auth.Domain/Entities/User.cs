using System.ComponentModel.DataAnnotations;

namespace Auth.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Roles Role { get; set; }
    public int TotalBalance { get; set; } = 0;
    public int TotalPoints { get; set; } = 0;
    public Guid RefreshToken { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    
    protected User() => Id = Guid.NewGuid();

    public User(string email, string passwordHash)
    {
        Email = email;
        PasswordHash = passwordHash;
    }
    
    public void UpdateUser(User newUser)
    {
        Email = newUser.Email;
        PasswordHash = newUser.PasswordHash;
        Role = newUser.Role;
    }

    public void GenerateRefreshToken()
    {
        RefreshToken = Guid.NewGuid();
    }
}