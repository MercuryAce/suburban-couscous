namespace VisitManagement.Api.Auth;

public sealed class AuthClientOptions
{
    public string ClientId { get; set; } = "";
    public string SecretHash { get; set; } = "";
    public string[] Scopes { get; set; } = [];
}
