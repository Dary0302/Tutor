using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tutor.Application.Auth;
using Tutor.Application.Goals;
using Tutor.Controllers.Goals;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Users;
using Tutor.Tests.TestDoubles;

namespace Tutor.Tests.Api;

public sealed class GoalsApiTests
{
    private WebApplication app = null!;
    private HttpClient client = null!;
    private GoalTestRepository repository = null!;

    [OneTimeSetUp]
    public async Task StartApi()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused",
            ["Jwt:Issuer"] = "Tutor.Api", ["Jwt:Audience"] = "Tutor",
            ["Jwt:SigningKey"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(32))
        });
        new Startup(builder.Configuration).ConfigureServices(builder.Services);
        builder.Services.AddSingleton<IGoalRepository, GoalTestRepository>();
        builder.Services.AddControllers().AddApplicationPart(typeof(GoalsController).Assembly);
        app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        repository = (GoalTestRepository)app.Services.GetRequiredService<IGoalRepository>();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        client = new HttpClient { BaseAddress = new Uri(address) };
    }

    [SetUp]
    public void Reset() => repository.Reset();

    [OneTimeTearDown]
    public async Task StopApi()
    {
        client?.Dispose();
        await app.DisposeAsync();
    }

    [Test]
    public async Task FullWorkflowWorksThroughAuthenticatedEndpoints()
    {
        using var createdResponse = await Send(UserRole.Mentor, "POST", "/api/goals", repository.NewGoal());
        Assert.That(createdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var goal = (await createdResponse.Content.ReadFromJsonAsync<GoalDto>())!;
        foreach (var task in goal.Tasks)
        {
            using var response = await Send(UserRole.Student, "PUT", $"/api/goals/{goal.Id}/tasks/{task.Id}/completion",
                new SetGoalTaskCompletionRequest { IsCompleted = true });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        using var reviewResponse = await Send(UserRole.Student, "GET", $"/api/goals/{goal.Id}");
        var reviewed = (await reviewResponse.Content.ReadFromJsonAsync<GoalDto>())!;
        Assert.That(reviewed.Status, Is.EqualTo(GoalStatus.PendingReview));
        using var progressResponse = await Send(UserRole.Student, "GET", $"/api/goals/progress?PairId={repository.Pair.Id}");
        var progress = (await progressResponse.Content.ReadFromJsonAsync<List<SkillProgressDto>>())!;
        Assert.That(progress.Single().Percentage, Is.EqualTo(100));
        using var confirmedResponse = await Send(UserRole.Mentor, "POST", $"/api/goals/{goal.Id}/confirm");
        Assert.That(confirmedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await confirmedResponse.Content.ReadFromJsonAsync<GoalDto>())!.Status, Is.EqualTo(GoalStatus.Completed));
    }

    [TestCase(UserRole.Student, "POST", "/api/goals")]
    [TestCase(UserRole.Student, "POST", "/api/goals/{goal}/tasks")]
    [TestCase(UserRole.Student, "DELETE", "/api/goals/{goal}/tasks/{task}")]
    [TestCase(UserRole.Student, "POST", "/api/goals/{goal}/confirm")]
    [TestCase(UserRole.Student, "DELETE", "/api/goals/{goal}")]
    [TestCase(UserRole.Student, "GET", "/api/goals/students")]
    [TestCase(UserRole.Mentor, "PUT", "/api/goals/{goal}/tasks/{task}/completion")]
    [TestCase(UserRole.Tutor, "GET", "/api/goals")]
    [TestCase(UserRole.Tutor, "GET", "/api/goals/progress")]
    public async Task RoleRestrictionsRejectRequestsBeforeDatabase(UserRole role, string method, string route)
    {
        route = route.Replace("{goal}", Guid.NewGuid().ToString()).Replace("{task}", Guid.NewGuid().ToString());
        using var response = await Send(role, method, route, new { });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(repository.ReadCount, Is.Zero);
    }

    [TestCase("/api/goals")]
    [TestCase("/api/goals/progress")]
    [TestCase("/api/goals/students")]
    public async Task AnonymousRequestsAreRejected(string route)
    {
        using var response = await client.GetAsync(route);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase("Page=0")]
    [TestCase("PageSize=101")]
    [TestCase("Page=2147483647&PageSize=100")]
    [TestCase("Status=999")]
    [TestCase("PairId=00000000-0000-0000-0000-000000000000")]
    public async Task InvalidListQueriesAreValidatedAutomatically(string query)
    {
        using var response = await Send(UserRole.Student, "GET", "/api/goals?" + query);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(repository.ReadCount, Is.Zero);
    }

    [TestCase(0)]
    [TestCase(101)]
    public async Task GoalRequiresOneToOneHundredTasks(int taskCount)
    {
        using var response = await Send(UserRole.Mentor, "POST", "/api/goals", repository.NewGoal(taskCount));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(repository.ReadCount, Is.Zero);
    }

    [Test]
    public async Task BlankTaskAndMissingCompletionValueAreRejected()
    {
        using var taskResponse = await Send(UserRole.Mentor, "POST", $"/api/goals/{Guid.NewGuid()}/tasks", new { name = "  " });
        using var completionResponse = await Send(UserRole.Student, "PUT",
            $"/api/goals/{Guid.NewGuid()}/tasks/{Guid.NewGuid()}/completion", new { });
        Assert.That(taskResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(completionResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(repository.ReadCount, Is.Zero);
    }

    [Test]
    public async Task AnotherStudentCannotReadOrCompleteTheGoal()
    {
        using var createdResponse = await Send(UserRole.Mentor, "POST", "/api/goals", repository.NewGoal());
        var goal = (await createdResponse.Content.ReadFromJsonAsync<GoalDto>())!;
        var other = Guid.NewGuid();
        using var readResponse = await Send(UserRole.Student, "GET", $"/api/goals/{goal.Id}", userId: other);
        using var completeResponse = await Send(UserRole.Student, "PUT", $"/api/goals/{goal.Id}/tasks/{goal.Tasks[0].Id}/completion",
            new { isCompleted = true }, other);
        Assert.That(readResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(completeResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(repository.Goal!.Tasks.Count(t => t.Status == GoalTaskStatus.Completed), Is.Zero);
    }

    [Test]
    public async Task MentorCanLoadStudentsAndSkillCatalogAndStudentCanLoadPagedGoals()
    {
        using var studentsResponse = await Send(UserRole.Mentor, "GET", "/api/goals/students");
        Assert.That(studentsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var students = (await studentsResponse.Content.ReadFromJsonAsync<GoalPage<GoalStudentDto>>())!;
        Assert.That(students.Items.Single().PairId, Is.EqualTo(repository.Pair.Id));
        using var skillsResponse = await Send(UserRole.Mentor, "GET", "/api/goals/skills?Query=HTML");
        Assert.That(skillsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var goalsResponse = await Send(UserRole.Student, "GET", "/api/goals?Page=2&PageSize=7");
        Assert.That(goalsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var page = (await goalsResponse.Content.ReadFromJsonAsync<GoalPage<GoalDto>>())!;
        Assert.That(page.PageSize, Is.EqualTo(7));
        Assert.That(page.Page, Is.EqualTo(2));
    }

    private async Task<HttpResponseMessage> Send(UserRole role, string method, string route, object? body = null, Guid? userId = null)
    {
        var user = new User
        {
            Id = userId ?? (role == UserRole.Mentor ? repository.Pair.MentorId : repository.Pair.StudentId),
            Email = "user@example.test", PasswordHash = "", Role = role
        };
        using var request = new HttpRequestMessage(new HttpMethod(method), route);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            app.Services.GetRequiredService<IAccessTokenIssuer>().Issue(user));
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return await client.SendAsync(request);
    }
}
