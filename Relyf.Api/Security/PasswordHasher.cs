using System.Security.Cryptography;

namespace Relyf.Api.Security;

public static class PasswordHasher
{
    public static (byte[] hash, byte[] salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16); // 128-bit
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA512);
        return (pbkdf2.GetBytes(64), salt);            // 512-bit hash
    }

    public static bool Verify(string password, byte[] salt, byte[] expectedHash)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA512);
        var hash = pbkdf2.GetBytes(64);
        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }
}
