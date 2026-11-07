using Microsoft.Extensions.DependencyInjection;
using Tutor.Application.Article;
using Tutor.Application.Auth;

namespace Tutor.Application.Configurations;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<ArticleService>();
        return services;
    }
}
