using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tutor.Application.Questionnaires;
using Tutor.Domain.Billing;
using Tutor.Domain.Articles;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Repositories;
using Tutor.Domain.Users;
using Tutor.Infrastructure.Repositories;
using Tutor.Infrastructure.Users;

namespace Tutor.Infrastructure.Configurations;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new NullReferenceException("No database connection string found.");

        services.AddDbContextFactory<TutorDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();

        services.AddGuidRepository<Skill>();
        services.AddGuidRepository<Questionnaire>();
        services.AddGuidRepository<TeacherInfo>();
        services.AddGuidRepository<AvailabilitySlot>();
        services.AddGuidRepository<AccountTransaction>();
        services.AddGuidRepository<Article>();
        services.AddGuidRepository<MentorshipRequest>();
        services.AddGuidRepository<MentorshipPair>();
        services.AddGuidRepository<LearningGoal>();
        services.AddGuidRepository<GoalTask>();
        services.AddGuidRepository<MentoringSession>();
        services.AddRepository<QuestionnaireSkill, QuestionnaireSkillId>(id => [id.SkillId, id.QuestionnaireId]);
        return services;
    }

    private static void AddGuidRepository<TEntity>(this IServiceCollection services) where TEntity : class =>
        services.AddRepository<TEntity, Guid>(id => [id]);

    private static void AddRepository<TEntity, TKey>(this IServiceCollection services, Func<TKey, object?[]> keyValues)
        where TEntity : class =>
        services.AddScoped<IRepository<TEntity, TKey>>(provider =>
            new EfRepository<TEntity, TKey>(provider.GetRequiredService<IDbContextFactory<TutorDbContext>>(), keyValues));
}
