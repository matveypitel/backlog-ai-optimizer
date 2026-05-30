using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Auth;

using Microsoft.Extensions.Options;

namespace BacklogOptimizer.UnitTests.Infrastructure.Auth;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;
    private readonly JwtSettings _settings;

    public JwtTokenServiceTests()
    {
        _settings = new JwtSettings
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningKey = "super-secret-key-that-is-at-least-32-bytes-long",
            AccessTokenMinutes = 60
        };

        _service = new JwtTokenService(Options.Create(_settings));
    }

    [Fact]
    public void Generate_ReturnsNonEmptyToken()
    {
        var user = CreateUser(Role.User);

        var result = _service.Generate(user);

        Assert.NotEmpty(result.AccessToken);
    }

    [Fact]
    public void Generate_ExpiresAt_IsInFuture()
    {
        var user = CreateUser(Role.User);
        var before = DateTime.UtcNow;

        var result = _service.Generate(user);

        Assert.True(result.ExpiresAtUtc > before);
        Assert.True(result.ExpiresAtUtc <= before.AddMinutes(_settings.AccessTokenMinutes + 1));
    }

    [Fact]
    public void Generate_ReturnsCorrectEmailAndRole()
    {
        var user = CreateUser(Role.Admin);

        var result = _service.Generate(user);

        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public void Generate_TokenContainsSubClaim_EqualToUserId()
    {
        var user = CreateUser(Role.User);
        user.Id = Guid.NewGuid();

        var result = _service.Generate(user);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);
        var sub = token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;

        Assert.Equal(user.Id.ToString(), sub);
    }

    [Fact]
    public void Generate_TokenContainsEmailClaim()
    {
        var user = CreateUser(Role.User);

        var result = _service.Generate(user);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);
        var email = token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;

        Assert.Equal("test@example.com", email);
    }

    [Fact]
    public void Generate_TokenContainsRoleClaim()
    {
        var user = CreateUser(Role.Admin);

        var result = _service.Generate(user);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);
        var role = token.Claims.First(c => c.Type == ClaimTypes.Role).Value;

        Assert.Equal("Admin", role);
    }

    [Fact]
    public void Generate_TokenHasCorrectIssuerAndAudience()
    {
        var user = CreateUser(Role.User);

        var result = _service.Generate(user);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);

        Assert.Equal(_settings.Issuer, token.Issuer);
        Assert.Contains(_settings.Audience, token.Audiences);
    }

    private static User CreateUser(Role role)
    {
        var user = new User("test@example.com", "hash", role);
        return user;
    }
}
