using Core.Common.Extensions;
using Core.Common.Extensions.Validation;
using Core.Common.HttpLogic;
using Microsoft.AspNetCore.Identity;
using Tutor.Application.Configurations;
using Tutor.Authentication;
using Tutor.Domain.Users;
using Tutor.Infrastructure.Configurations;
using Tutor.Validators;

namespace Tutor;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddInfrastructureServices(configuration)
            .AddApplicationServices()
            .AddCorsPolicy()
            .AddExceptionHandler<ExceptionHandler>()
            .AddProblemDetails()
            .AddOpenApi("Tutor", typeof(ApplicationConfiguration))
            .AddAuthentication(configuration)
            .AddValidation<IValidationMarker>()
            .AddScoped<IPasswordHasher<User>, PasswordHasher<User>>()
            .AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        app
            .UseExceptionHandler()
            .UseRouting()
            .UseCorsPolicy()
            .UseAuthentication()
            .UseAuthorization()
            .UseOpenApi()
            .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                }
            );
    }
}
