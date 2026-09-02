using System.Security.Cryptography;
using System.Text;

namespace VisitManagement.Api.Auth;

public static class SecretHasher
{
    public static string Hash(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool Matches(string secret, string expectedHash) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Hash(secret)),
            Encoding.UTF8.GetBytes(expectedHash.ToLowerInvariant()));
}
