using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        
        var dbContext = serviceProvider.GetRequiredService<TutorDbContext>();
        
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<User>>();

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var curatorEmail = config["SeedData:CuratorEmail"];
        var curatorPassword = config["SeedData:CuratorPassword"];
        
        var tempUser = new User 
        { 
            Id = Guid.NewGuid(),
            Email = curatorEmail, 
            Role = UserRole.Tutor, 
            PasswordHash = "Temp" 
        };
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(curatorPassword);

        var existingUser = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == curatorEmail);

        if (existingUser == null)
        {
            logger.LogInformation("Куратор не найден. Создаем...");

            var curator = new User
            {
                Id = tempUser.Id,
                Email = curatorEmail,
                Role = UserRole.Tutor,
                DeletedAt = null,
                PasswordHash = passwordHash 
            };

            dbContext.Users.Add(curator);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Куратор успешно создан.");
        }
    }
}