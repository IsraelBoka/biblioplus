using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Penalite : BaseEntity
{
    public int AdherentId { get; set; }
    public Adherent? Adherent { get; set; }

    public int EmpruntId { get; set; }
    public Emprunt? Emprunt { get; set; }

    [Required]
    [MaxLength(200)]
    public string Motif { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Montant { get; set; }

    public EtatPenalite Etat { get; set; } = EtatPenalite.ARegler;
}
