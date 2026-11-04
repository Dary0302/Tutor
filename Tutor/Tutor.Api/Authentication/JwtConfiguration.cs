using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Tutor.Application.Auth;

namespace Tutor.Authentication;

internal static class JwtConfiguration
{
    private const string InvalidUserIdMessage = "Некорректный идентификатор пользователя.";

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException("Set Jwt:SigningKey to a secret of at least 32 UTF-8 bytes.");

        var parameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Missing Jwt:Issuer."),
            ValidAudience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Missing Jwt:Audience."),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Email,
            RoleClaimType = ClaimTypes.Role
        };
        services.AddSingleton<IAccessTokenIssuer>(new JwtAccessTokenIssuer(parameters));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = true;
                options.TokenValidationParameters = parameters;
                options.Events.OnTokenValidated = context =>
                {
                    if (!Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out _))
                        context.Fail(InvalidUserIdMessage);
                    return Task.CompletedTask;
                };
            });
        services.AddAuthorization();
        services.ConfigureSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT"
            });
            options.OperationFilter<AuthorizeOperationFilter>();
        });
        return services;
    }
}
