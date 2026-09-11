using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Web.Models;

/// <summary>Ligne d'affichage d'une pénalité (liste / détails).</summary>
public class PenaliteViewModel
{
    public int Id { get; set; }

    [Display(Name = "Adhérent")]
    public string AdherentNom { get; set; } = string.Empty;

    [Display(Name = "Emprunt")]
    public int EmpruntId { get; set; }

    [Display(Name = "Motif")]
    public string Motif { get; set; } = string.Empty;

    [Display(Name = "Montant")]
    public decimal Montant { get; set; }

    [Display(Name = "État")]
    public EtatPenalite Etat { get; set; }
}
