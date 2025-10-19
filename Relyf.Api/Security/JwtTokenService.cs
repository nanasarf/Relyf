using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Relyf.Api.Jwt;

namespace Relyf.Api.Security;

public interface IJwtTokenService
{
    string Create(int userId, string email, string displayName);
}

public sealed class JwtTokenService(IOptions<JwtOptions> opts) : IJwtTokenService
{
    private readonly JwtOptions _o = opts.Value;

    public string Create(int userId, string email, string displayName)
    {
        // Key is stored as Base64 in secrets: dotnet user-secrets set "Jwt:Key" "<base64>"
        var keyBytes = Convert.FromBase64String(_o.Key);
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("name", displayName)
        };

        var token = new JwtSecurityToken(
            issuer: _o.Issuer,
            audience: _o.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_o.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
