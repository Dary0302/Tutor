using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using Tutor.Application.Auth;
using Tutor.Domain.Users;

namespace Tutor.Tests.Api;

public sealed class JwtAuthenticationTests
{
    private const string Issuer = "Tutor.Api";
    private const string Audience = "Tutor";
    private readonly string signingKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private ServiceProvider services = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused",
            ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience,
            ["Jwt:SigningKey"] = signingKey
        }).Build();
        var collection = new ServiceCollection();
        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddLogging();
        new Startup(configuration).ConfigureServices(collection);
        services = collection.BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void TearDown() => services.Dispose();

    [TestCase(UserRole.Student)]
    [TestCase(UserRole.Mentor)]
    [TestCase(UserRole.Tutor)]
    public async Task IssuedTokenAuthenticatesWithTheUsersIdentityAndRole(UserRole role)
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.test", Role = role, PasswordHash = "" };
        var token = services.GetRequiredService<IAccessTokenIssuer>().Issue(user);
        var result = await AuthenticateAsync(token);

        Assert.That(result.Succeeded, Is.True);
        var principal = result.Principal!;
        Assert.That(principal.FindFirstValue(ClaimTypes.NameIdentifier), Is.EqualTo(user.Id.ToString()));
        Assert.That(principal.Identity!.Name, Is.EqualTo(user.Email));
        Assert.That(principal.IsInRole(role.ToString()), Is.True);
    }

    [TestCase("OtherIssuer", Audience, 5, true)]
    [TestCase(Issuer, "OtherAudience", 5, true)]
    [TestCase(Issuer, Audience, -5, true)]
    [TestCase(Issuer, Audience, 5, false)]
    public async Task RejectsInvalidBearerToken(string issuer, string audience, int minutesUntilExpiry, bool trustedKey)
    {
        var key = trustedKey ? signingKey : Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var token = new JwtSecurityToken(issuer, audience, [new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())],
            expires: DateTime.UtcNow.AddMinutes(minutesUntilExpiry),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        var result = await AuthenticateAsync(new JwtSecurityTokenHandler().WriteToken(token));
        Assert.That(result.Succeeded, Is.False);
    }

    private async Task<AuthenticateResult> AuthenticateAsync(string token)
    {
        using var scope = services.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Headers.Authorization = $"Bearer {token}";
        return await context.AuthenticateAsync();
    }
}
