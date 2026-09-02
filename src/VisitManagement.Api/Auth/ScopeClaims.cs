using System.Security.Claims;

namespace VisitManagement.Api.Auth;

public static class ScopeClaims
{
    public const string Read = "visits:read";
    public const string Write = "visits:write";
    public const string ReadPolicy = "visits:read";
    public const string WritePolicy = "visits:write";

    public static bool HasScope(ClaimsPrincipal user, string scope)
    {
        foreach (var claim in user.FindAll("scope").Concat(user.FindAll("scp")))
        {
            if (claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Contains(scope, StringComparer.Ordinal))
                return true;
        }

        return false;
    }
}
