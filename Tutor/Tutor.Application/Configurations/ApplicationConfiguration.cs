using Microsoft.Extensions.DependencyInjection;

namespace Tutor.Application.Configurations;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Так регистрируем сервисы
        // services
        //     .AddScoped<IStorageService, Implementations.StorageService>();
        
        return services;
    }
}