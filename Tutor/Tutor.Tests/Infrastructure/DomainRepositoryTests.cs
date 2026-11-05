using Core.Common.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NUnit.Framework;
using Tutor.Application.Configurations;
using Tutor.Application.Questionnaires;
using Tutor.Domain.Articles;
using Tutor.Domain.Billing;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Repositories;
using Tutor.Domain.Users;
using Tutor.Infrastructure.Configurations;
using Tutor.Infrastructure.Migrations;

namespace Tutor.Tests.Infrastructure;

public sealed class DomainRepositoryTests
{
    private readonly string database = $"tutor_test_{Guid.NewGuid():N}";
    private string? adminConnection;
    private IHost? host;
    private IServiceScope? scope;

    [OneTimeSetUp]
    public async Task CreateDatabase()
    {
        var connection = Environment.GetEnvironmentVariable("TUTOR_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connection))
            Assert.Ignore("Set TUTOR_TEST_POSTGRES to run repository tests against PostgreSQL.");

        await ExecuteAsync(connection!, $"CREATE DATABASE {database}");
        adminConnection = connection;
        var testConnection = new NpgsqlConnectionStringBuilder(connection)
        {
            Database = database, Pooling = false
        };
        host = new HostBuilder()
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = testConnection.ConnectionString
            }))
            .ConfigureServices((context, services) => services.AddInfrastructureServices(context.Configuration).AddApplicationServices())
            .Build();
        host.RunMigrations<MigrationMarker>();
    }

    [SetUp]
    public void OpenScope() => scope = host!.Services.CreateScope();

    [TearDown]
    public void CloseScope() => scope?.Dispose();

    [OneTimeTearDown]
    public async Task DropDatabase()
    {
        host?.Dispose();
        if (adminConnection is not null)
            await ExecuteAsync(adminConnection, $"DROP DATABASE {database} WITH (FORCE)");
    }

    [Test]
    public async Task SkillRepositorySupportsCrudAndFilters()
    {
        var skills = Repository<Skill>();
        var skill = new Skill { Name = "C#" };
        await skills.AddAsync(skill);
        skill.Name = "PostgreSQL";
        Assert.That(await skills.UpdateAsync(skill), Is.True);
        Assert.That((await skills.ListAsync(s => s.Name == skill.Name)).Select(s => s.Id), Does.Contain(skill.Id));
        Assert.That((await skills.GetByIdAsync(skill.Id))?.Name, Is.EqualTo(skill.Name));
        Assert.That(await skills.DeleteAsync(skill.Id), Is.True);
        Assert.That(await skills.GetByIdAsync(skill.Id), Is.Null);
        Assert.That(await skills.UpdateAsync(skill), Is.False);
        Assert.That(await skills.DeleteAsync(skill.Id), Is.False);
    }

    [Test]
    public async Task QuestionnaireLinksKeepTheirCompositeKeyAndFollowTheirOwner()
    {
        var questionnaire = new Questionnaire
        {
            UserId = await CreateUser(UserRole.Mentor), FirstName = "Ирина", LastName = "Ли",
            City = "Пермь", Education = "Университет", GoalDescription = "Обучать",
            CommunicationFormat = CommunicationFormat.Hybrid, MonthsNumberMin = 1, MonthsNumberMax = 3, HoursNumberWeek = 4
        };
        await Repository<Questionnaire>().AddAsync(questionnaire);
        var skill = new Skill { Name = "SQL" };
        await Repository<Skill>().AddAsync(skill);
        var skills = scope!.ServiceProvider.GetRequiredService<IRepository<QuestionnaireSkill, QuestionnaireSkillId>>();
        var key = new QuestionnaireSkillId(skill.Id, questionnaire.Id);
        var link = new QuestionnaireSkill { SkillId = skill.Id, QuestionnaireId = questionnaire.Id };
        await skills.AddAsync(link);
        await Assert.ThrowsAsync<DbUpdateException>(() => skills.AddAsync(link));
        link.SkillLevel = SkillLevel.Expert;
        Assert.That(await skills.UpdateAsync(link), Is.True);
        Assert.That((await skills.GetByIdAsync(key))?.SkillLevel, Is.EqualTo(SkillLevel.Expert));
        Assert.That(await skills.GetByIdAsync(new QuestionnaireSkillId(questionnaire.Id, skill.Id)), Is.Null);
        await Assert.ThrowsAsync<DbUpdateException>(() => Repository<Skill>().DeleteAsync(skill.Id));

        var teacher = new TeacherInfo
        {
            TeacherQuestionnaireId = questionnaire.Id,
            PlaceOfWork = "Школа", Position = "Преподаватель", YearsExperience = 5, MaxStudents = 3
        };
        await Repository<TeacherInfo>().AddAsync(teacher);
        Assert.That((await Repository<TeacherInfo>().GetByIdAsync(questionnaire.Id))?.Position, Is.EqualTo(teacher.Position));
        await Assert.ThrowsAsync<DbUpdateException>(() => Repository<TeacherInfo>().AddAsync(teacher));
        var slot = new AvailabilitySlot
        {
            QuestionnaireId = questionnaire.Id, DayOfWeek = DayOfWeek.Friday,
            StartTime = new TimeOnly(9, 30), EndTime = new TimeOnly(11, 0)
        };
        await Repository<AvailabilitySlot>().AddAsync(slot);
        Assert.That((await Repository<AvailabilitySlot>().GetByIdAsync(slot.Id))?.StartTime, Is.EqualTo(slot.StartTime));
        Assert.That(await Repository<Questionnaire>().DeleteAsync(questionnaire.Id), Is.True);
        Assert.That(await skills.GetByIdAsync(key), Is.Null);
        Assert.That(await Repository<TeacherInfo>().GetByIdAsync(questionnaire.Id), Is.Null);
        Assert.That(await Repository<AvailabilitySlot>().GetByIdAsync(slot.Id), Is.Null);
        Assert.That(await Repository<Skill>().GetByIdAsync(skill.Id), Is.Not.Null);
    }

    [Test]
    public async Task PairConnectsARequestWithGoalsTasksAndSessions()
    {
        var request = new MentorshipRequest
        {
            StudentId = await CreateUser(UserRole.Student), TeacherId = await CreateUser(UserRole.Mentor),
            CuratorId = await CreateUser(UserRole.Tutor), StudentLetter = "Хочу изучить SQL", Status = RequestStatus.Accepted
        };
        await Repository<MentorshipRequest>().AddAsync(request);
        var pair = new MentorshipPair { RequestId = request.Id };
        await Repository<MentorshipPair>().AddAsync(pair);
        var duplicatePair = new MentorshipPair { RequestId = request.Id };
        await Assert.ThrowsAsync<DbUpdateException>(() => Repository<MentorshipPair>().AddAsync(duplicatePair));
        var skill = new Skill { Name = "Работа с БД" };
        await Repository<Skill>().AddAsync(skill);
        var goal = new LearningGoal { PairId = pair.Id, SkillId = skill.Id, Title = "SQL", Description = "Написать запрос" };
        await Repository<LearningGoal>().AddAsync(goal);
        var task = new GoalTask { GoalId = goal.Id, Name = "SELECT" };
        await Repository<GoalTask>().AddAsync(task);
        var session = new MentoringSession
        {
            PairId = pair.Id, Agenda = "Разбор запросов", StartsAt = DateTimeOffset.UtcNow,
            EndsAt = DateTimeOffset.UtcNow.AddHours(1)
        };
        await Repository<MentoringSession>().AddAsync(session);
        var payment = new AccountTransaction
        {
            UserId = request.StudentId, Reason = TransactionReason.Payment, Description = "Занятие", BalanceAmount = -10
        };
        await Repository<AccountTransaction>().AddAsync(payment);

        Assert.That((await Repository<MentorshipPair>().GetByIdAsync(pair.Id))?.RequestId, Is.EqualTo(request.Id));
        Assert.That((await Repository<MentoringSession>().GetByIdAsync(session.Id))?.PairId, Is.EqualTo(pair.Id));
        Assert.That((await Repository<LearningGoal>().GetByIdAsync(goal.Id))?.SkillId, Is.EqualTo(skill.Id));
        Assert.That((await Repository<GoalTask>().GetByIdAsync(task.Id))?.GoalId, Is.EqualTo(goal.Id));
        Assert.That((await Repository<AccountTransaction>().GetByIdAsync(payment.Id))?.BalanceAmount, Is.EqualTo(-10));
        await Assert.ThrowsAsync<DbUpdateException>(() => Repository<MentorshipPair>().DeleteAsync(pair.Id));
    }

    [Test]
    public async Task ArticleRepositoryStoresMetadataAndRequiresAnExistingAuthor()
    {
        var articles = Repository<Article>();
        var article = new Article { AuthorId = Guid.NewGuid(), Title = "Основы SQL", S3Key = "articles/sql.md" };
        await Assert.ThrowsAsync<DbUpdateException>(() => articles.AddAsync(article));
        article.AuthorId = await CreateUser(UserRole.Mentor);
        await articles.AddAsync(article);
        var stored = (await articles.ListAsync(a => a.AuthorId == article.AuthorId)).Single();
        Assert.That(stored.Id, Is.EqualTo(article.Id));
        Assert.That(stored.Title, Is.EqualTo(article.Title));
        Assert.That(stored.S3Key, Is.EqualTo(article.S3Key));
    }

    [Test]
    public async Task ConcurrentFormSavesReuseCatalogSkillsAndReplaceOnlyTheOwnersAnswers()
    {
        var service = scope!.ServiceProvider.GetRequiredService<IQuestionnaireService>();
        var mentor = await CreateUser(UserRole.Mentor);
        var student = await CreateUser(UserRole.Student);
        var skillName = $"Нейросети-{Guid.NewGuid():N}";
        var slot = new TimeWindowDto { DayOfWeek = DayOfWeek.Friday, StartTime = new(10, 0), EndTime = new(12, 0) };
        var teacher = new MentorInfoDto { PlaceOfWork = "Школа", Position = "Наставник", YearsExperience = 3, MaxStudents = 2 };

        QuestionnaireFormDto Form(string name, MentorInfoDto? info = null) => new()
        {
            FirstName = "Ирина", LastName = "Ли", City = "Пермь", Education = "Вуз", GoalDescription = "Изучить ML",
            Skills = [new() { Name = name }], Availability = [slot], TeacherInfo = info
        };

        var results = await Task.WhenAll(
            service.SaveAsync(mentor, UserRole.Mentor, Form(skillName, teacher), default),
            service.SaveAsync(student, UserRole.Student, Form(skillName.ToLowerInvariant()), default),
            service.SaveAsync(mentor, UserRole.Mentor, Form(skillName, teacher), default));
        Assert.That(results.All(r => r.IsSuccess), Is.True);
        var original = (await Repository<Questionnaire>().ListAsync(q => q.UserId == mentor)).Single();
        Assert.That((await Repository<Skill>().ListAsync()).Count(s => string.Equals(s.Name, skillName, StringComparison.OrdinalIgnoreCase)), Is.EqualTo(1));

        var edit = Form("Другой навык", teacher);
        edit.Availability.Clear();
        Assert.That((await service.SaveAsync(mentor, UserRole.Mentor, edit, default)).IsSuccess, Is.True);
        Assert.That((await Repository<Questionnaire>().ListAsync(q => q.UserId == mentor)).Single().Id, Is.EqualTo(original.Id));
        var saved = (await service.GetAsync(mentor, default)).Value;
        Assert.That(saved.Availability, Is.Empty);
        Assert.That(saved.Skills.Single().Name, Is.EqualTo("Другой навык"));
        Assert.That(saved.TeacherInfo?.MaxStudents, Is.EqualTo(2));
        var other = (await service.GetAsync(student, default)).Value;
        Assert.That(other.Skills.Single().Name, Is.EqualTo(skillName).IgnoreCase);
        Assert.That(other.Availability.Single().StartTime, Is.EqualTo(slot.StartTime));
        Assert.That(other.TeacherInfo, Is.Null);
    }

    private IRepository<TEntity, Guid> Repository<TEntity>() where TEntity : class =>
        scope!.ServiceProvider.GetRequiredService<IRepository<TEntity, Guid>>();

    private async Task<Guid> CreateUser(UserRole role)
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, Email = $"{id}@example.test", PasswordHash = "test", Role = role };
        await scope!.ServiceProvider.GetRequiredService<IUserRepository>().TryCreateAsync(user, default);
        return id;
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
