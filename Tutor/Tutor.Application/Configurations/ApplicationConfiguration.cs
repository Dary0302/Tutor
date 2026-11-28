using Microsoft.Extensions.DependencyInjection;
using Tutor.Application.Applications;
using Tutor.Application.Article;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tutor.Application.Goals;
using Tutor.Application.Auth;
using Tutor.Application.Mentors;
using Tutor.Application.Questionnaires;
using Tutor.Application.Sessions;
using Tutor.Domain.Applications;
using Tutor.Domain.Articles;

namespace Tutor.Application.Configurations;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        return services.AddScoped<AuthService>()
            .AddScoped<IArticleService, ArticleService>()
            .AddScoped<IGoalService, GoalService>()
            .AddScoped<IGoalProgressService, GoalProgressService>()
            .AddScoped<IGoalWorkflowService, GoalWorkflowService>()
            .AddScoped<IQuestionnaireService, QuestionnaireService>()
            .AddScoped<ISessionService, SessionService>()
            .AddScoped<IMentorSearchService, MentorSearchService>()
            .AddScoped<IMentorFindService, MentorFindService>()
            .AddScoped<IMentorCompatibilityService, MentorCompatibilityService>()
            .AddScoped<IAppService, AppService>()
            .AddAutoMapper(_ => { }, typeof(QuestionnaireProfile));
    }
}
