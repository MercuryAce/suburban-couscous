using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VisitManagement.Api.Auth;

namespace VisitManagement.Api.Tests;

internal static class TestJwt
{
    public const string Issuer = "visit-management";
    public const string Audience = "visit-management";
    public const string SigningKey = "visit-management-test-signing-key!";
    public const string ClientId = "test-client";
    public const string ClientSecret = "test-secret";

    public static string ClientSecretHash { get; } = SecretHasher.Hash(ClientSecret);

    public static string CreateToken(params string[] scopes) =>
        CreateToken(ClientId, scopes);

    public static string CreateToken(string subject, IReadOnlyList<string> scopes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var token = new JwtSecurityToken(
            Issuer,
            Audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, subject),
                new Claim("scope", string.Join(' ', scopes))
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
