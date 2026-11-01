using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Application.Shared;
using System.Reflection;

namespace Auth.Application.Configuration;

public static class ServiceExtensions
{
    public static void ConfigureApplicationApp(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
    }
}