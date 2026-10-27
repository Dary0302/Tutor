using Core.Common.Extensions;
using Core.Common.Extensions.Validation;
using Core.Common.HttpLogic;
using Tutor.Application.Configurations;
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
            .AddValidation<IValidationMarker>()
            .AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        app
            .UseExceptionHandler()
            .UseRouting()
            .UseCorsPolicy()
            .UseOpenApi()
            .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                }
            );
    }
}