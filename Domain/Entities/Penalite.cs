using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Pénalité de retard rattachée à un emprunt et à son adhérent.
/// </summary>
public class Penalite : BaseEntity
{
    // Relation 1—* : Adherent -> Penalite
    public int AdherentId { get; set; }
    public Adherent? Adherent { get; set; }

    // Relation 1—0..1 : Emprunt -> Penalite (une pénalité découle d'un emprunt précis)
    public int EmpruntId { get; set; }
    public Emprunt? Emprunt { get; set; }

    [Required]
    [MaxLength(200)]
    public string Motif { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Montant { get; set; }

    public EtatPenalite Etat { get; set; } = EtatPenalite.ARegler;
}
