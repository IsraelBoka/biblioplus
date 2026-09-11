namespace Application.Abstractions;

/// <summary>
/// Hachage et vérification des mots de passe. Implémentation dans l'Infrastructure
/// (aucune dépendance cryptographique dans la couche Application).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string motDePasse);
    bool Verify(string motDePasse, string hash);
}
