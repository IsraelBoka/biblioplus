using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Compte de connexion. Un compte Admin gère la bibliothèque ;
/// un compte Adherent est rattaché à un <see cref="Adherent"/> et accède à son espace personnel.
/// </summary>
public class Utilisateur : BaseEntity
{
    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasseHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    public RoleUtilisateur Role { get; set; } = RoleUtilisateur.Adherent;

    public bool Actif { get; set; } = true;

    // Rattachement à un adhérent (uniquement pour les comptes de rôle Adherent).
    public int? AdherentId { get; set; }
    public Adherent? Adherent { get; set; }
}
