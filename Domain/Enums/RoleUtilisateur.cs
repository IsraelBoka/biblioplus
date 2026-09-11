namespace Domain.Enums;

/// <summary>
/// Rôle d'un compte utilisateur : détermine l'espace accessible après connexion.
/// </summary>
public enum RoleUtilisateur
{
    /// <summary>Bibliothécaire : accès complet à la gestion (espace admin).</summary>
    Admin = 0,

    /// <summary>Adhérent : accès à son espace personnel (catalogue, ses emprunts, ses pénalités).</summary>
    Adherent = 1
}
