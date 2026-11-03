using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tutor.Domain.Users;
using Tutor.Infrastructure.Users;

namespace Tutor.Infrastructure.Configurations;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new NullReferenceException("No database connection string found.");

        services.AddDbContext<TutorDbContext>(options => options.UseNpgsql(connectionString));

        return services.AddScoped<IUserRepository, UserRepository>();
    }
}
