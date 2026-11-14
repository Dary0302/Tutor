using Microsoft.Extensions.DependencyInjection;
using Tutor.Application.Auth;
using Tutor.Application.Mentors;
using Tutor.Application.Questionnaires;

namespace Tutor.Application.Configurations;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services.AddScoped<AuthService>()
            .AddScoped<IQuestionnaireService, QuestionnaireService>()
            .AddScoped<IMentorSearchService, MentorSearchService>()
            .AddScoped<IMentorCompatibilityService, MentorCompatibilityService>()
            .AddAutoMapper(_ => { }, typeof(QuestionnaireProfile));
    }
}
