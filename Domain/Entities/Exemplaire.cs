using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Copie physique d'un livre. C'est l'exemplaire (et non le livre) qui est emprunté.
/// </summary>
public class Exemplaire : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string CodeBarres { get; set; } = string.Empty;

    /// <summary>État matériel décrit librement (ex. « Bon », « Usé »).</summary>
    [MaxLength(100)]
    public string Etat { get; set; } = "Bon";

    public StatutExemplaire Statut { get; set; } = StatutExemplaire.Disponible;

    // Relation 1—* : Livre -> Exemplaire
    public int LivreId { get; set; }
    public Livre? Livre { get; set; }

    // Navigation : historique des emprunts de cet exemplaire.
    public ICollection<Emprunt> Emprunts { get; set; } = new List<Emprunt>();
}
