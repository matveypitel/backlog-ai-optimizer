using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Core.Entities;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BacklogOptimizer.Infrastructure.Auth;

internal sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public AuthTokenResult Generate(User user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.AccessTokenMinutes);
        var roleName = user.Role.ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, roleName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthTokenResult(encoded, expires, user.Email, roleName);
    }
}
