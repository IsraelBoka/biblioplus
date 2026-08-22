using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Référentiel : catégorie d'un livre. Détermine la durée maximale d'emprunt
/// et la pénalité journalière appliquée en cas de retard.
/// </summary>
public class CategorieLivre : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Libelle { get; set; } = string.Empty;

    /// <summary>Durée maximale d'emprunt, en jours.</summary>
    [Range(1, 365)]
    public int DureeMaxJours { get; set; }

    /// <summary>Pénalité par jour de retard commencé.</summary>
    [Range(0, 10000)]
    public decimal PenaliteParJour { get; set; }

    // Navigation : une catégorie classe plusieurs livres.
    public ICollection<Livre> Livres { get; set; } = new List<Livre>();
}
