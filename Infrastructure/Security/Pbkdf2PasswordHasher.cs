using System.Security.Cryptography;
using Application.Abstractions;

namespace Infrastructure.Security;

/// <summary>
/// Hachage PBKDF2 (SHA-256) autonome — aucune dépendance externe.
/// Format stocké : {iterations}.{selBase64}.{hashBase64}.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;      // 128 bits
    private const int KeySize = 32;       // 256 bits
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algo = HashAlgorithmName.SHA256;

    public string Hash(string motDePasse)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(motDePasse, salt, Iterations, Algo, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string motDePasse, string hash)
    {
        var parts = hash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var stored = Convert.FromBase64String(parts[2]);
        var calcule = Rfc2898DeriveBytes.Pbkdf2(motDePasse, salt, iterations, Algo, stored.Length);
        return CryptographicOperations.FixedTimeEquals(calcule, stored);
    }
}
