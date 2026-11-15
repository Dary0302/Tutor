using Microsoft.Extensions.DependencyInjection;
using Tutor.Application.Applications;
using Tutor.Application.Article;
using Tutor.Application.Auth;
using Tutor.Application.Mentors;
using Tutor.Application.Questionnaires;
using Tutor.Domain.Applications;

namespace Tutor.Application.Configurations;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services.AddScoped<AuthService>()
            .AddScoped<ArticleService>()
            .AddScoped<IQuestionnaireService, QuestionnaireService>()
            .AddScoped<IMentorSearchService, MentorSearchService>()
            .AddScoped<IMentorCompatibilityService, MentorCompatibilityService>()
            .AddScoped<IAppService, AppService>()
            .AddAutoMapper(_ => { }, typeof(QuestionnaireProfile));
    }
}
