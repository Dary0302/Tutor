using Microsoft.Extensions.DependencyInjection;
using Tutor.Application.Auth;

namespace Tutor.Application.Configurations;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services.AddScoped<AuthService>();
    }
}
