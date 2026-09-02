using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VisitManagement.Api.Auth;

namespace VisitManagement.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    IOptions<List<AuthClientOptions>> clients,
    TokenIssuer tokenIssuer) : ControllerBase
{
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<TokenResponse> Token([FromBody] TokenRequest request)
    {
        var client = clients.Value.FirstOrDefault(c =>
            string.Equals(c.ClientId, request.ClientId, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(c.SecretHash)
            && SecretHasher.Matches(request.ClientSecret, c.SecretHash));

        if (client is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid client credentials."
            });
        }

        return Ok(tokenIssuer.Issue(client.ClientId, client.Scopes));
    }
}
