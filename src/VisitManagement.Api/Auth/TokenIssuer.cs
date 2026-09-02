using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace VisitManagement.Api.Auth;

public sealed class TokenIssuer(IOptions<JwtOptions> options)
{
    public TokenResponse Issue(string clientId, IReadOnlyList<string> scopes)
    {
        var jwt = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var expires = DateTime.UtcNow.AddMinutes(jwt.LifetimeMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, clientId),
            new("scope", string.Join(' ', scopes))
        };

        var token = new JwtSecurityToken(
            jwt.Issuer,
            jwt.Audience,
            claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            jwt.LifetimeMinutes * 60);
    }
}

public sealed record TokenRequest(string ClientId, string ClientSecret);

public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
