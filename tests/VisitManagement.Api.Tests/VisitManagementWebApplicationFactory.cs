using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using VisitManagement.Api.Auth;

namespace VisitManagement.Api.Tests;

public sealed class VisitManagementWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Issuer", TestJwt.Issuer);
        builder.UseSetting("Jwt:Audience", TestJwt.Audience);
        builder.UseSetting("Jwt:SigningKey", TestJwt.SigningKey);
        builder.UseSetting("Jwt:LifetimeMinutes", "60");
        builder.UseSetting("AuthClients:0:ClientId", TestJwt.ClientId);
        builder.UseSetting("AuthClients:0:SecretHash", TestJwt.ClientSecretHash);
        builder.UseSetting("AuthClients:0:Scopes:0", ScopeClaims.Read);
        builder.UseSetting("AuthClients:0:Scopes:1", ScopeClaims.Write);
        builder.UseSetting("RateLimiting:PermitLimit", "10000");
    }
}
