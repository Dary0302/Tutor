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
using Tutor.Application.Mentors;
using Tutor.Controllers.Mentors;
using Tutor.Domain.Users;
using FluentResults;

namespace Tutor.Tests.Api;

public sealed class MentorSearchApiTests
{
    private WebApplication app = null!;
    private HttpClient client = null!;
    private readonly SearchService service = new();

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
        builder.Services.AddSingleton<IMentorSearchService>(service);
        builder.Services.AddControllers().AddApplicationPart(typeof(MentorsController).Assembly);
        app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        client = new HttpClient { BaseAddress = new Uri(address) };
    }

    [OneTimeTearDown]
    public async Task StopApi()
    {
        client?.Dispose();
        if (app is not null)
            await app.DisposeAsync();
    }

    [Test]
    public async Task AnonymousSearchIsRejected()
    {
        using var response = await client.GetAsync("/api/mentors");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase(UserRole.Student)]
    [TestCase(UserRole.Mentor)]
    [TestCase(UserRole.Tutor)]
    public async Task EveryRoleCanSearchWithBoundFilters(UserRole role)
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.test", PasswordHash = "", Role = role };
        using var request = AuthorizedRequest(user,
            "/api/mentors?Page=2&PageSize=7&DaysOfWeek=Monday&DaysOfWeek=Friday&MinHoursPerWeek=1.5&Sort=Rating");
        using var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(service.UserId, Is.EqualTo(user.Id));
        Assert.That(service.Request!.Page, Is.EqualTo(2));
        Assert.That(service.Request.PageSize, Is.EqualTo(7));
        var result = await response.Content.ReadFromJsonAsync<MentorSearchResult>();
        Assert.That(result!.PageSize, Is.EqualTo(7));
        Assert.That(service.Request.DaysOfWeek, Is.EqualTo(new[] { DayOfWeek.Monday, DayOfWeek.Friday }));
        Assert.That(service.Request.MinHoursPerWeek, Is.EqualTo(1.5m));
        Assert.That(service.Request.Sort, Is.EqualTo(MentorSort.Rating));
    }

    [TestCase("Page=0")]
    [TestCase("PageSize=0")]
    [TestCase("PageSize=-1")]
    [TestCase("PageSize=101")]
    [TestCase("Page=2147483647&PageSize=100")]
    [TestCase("SkillIds=not-a-guid")]
    [TestCase("DaysOfWeek=8")]
    [TestCase("Sort=999")]
    [TestCase("MinRating=6")]
    [TestCase("MinYearsExperience=10&MaxYearsExperience=1")]
    [TestCase("MinHoursPerWeek=5&MaxHoursPerWeek=1")]
    public async Task InvalidQueryReturnsBadRequestBeforeService(string query)
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.test", PasswordHash = "", Role = UserRole.Student };
        service.Request = null;
        using var request = AuthorizedRequest(user, "/api/mentors?" + query);
        using var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(service.Request, Is.Null);
    }

    [Test]
    public async Task OmittedPaginationUsesFirstPageOfTenMentors()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.test", PasswordHash = "", Role = UserRole.Student };
        using var request = AuthorizedRequest(user, "/api/mentors");
        using var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<MentorSearchResult>();
        Assert.That(result!.Page, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
        Assert.That(result.TotalPages, Is.Zero);
    }

    private HttpRequestMessage AuthorizedRequest(User user, string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            app.Services.GetRequiredService<IAccessTokenIssuer>().Issue(user));
        return request;
    }

    private sealed class SearchService : IMentorSearchService
    {
        public Guid UserId { get; private set; }
        public MentorSearchRequest? Request { get; set; }

        public Task<Result<MentorSearchResult>> SearchAsync(Guid userId, MentorSearchRequest request, CancellationToken ct)
        {
            UserId = userId;
            Request = request;
            return Task.FromResult(Result.Ok(new MentorSearchResult([], 0, request.Page, request.PageSize)));
        }
    }
}
