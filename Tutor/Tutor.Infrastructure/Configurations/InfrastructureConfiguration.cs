using Core.Common.DbHelpers;
using Core.Common.DbHelpers.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tutor.Infrastructure.Configurations;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new NullReferenceException("No database connection string found.");

        services.AddSingleton<IPostgresConnectionFactory>(new PostgresConnectionFactory(connectionString));

        // Так добавляем репозитории
        // services.AddSingleton<IStorageRepository, StorageRepository>();
    
        return services;
    }
}