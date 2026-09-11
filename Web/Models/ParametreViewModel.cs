using System.ComponentModel.DataAnnotations;

namespace Web.Models;

/// <summary>Formulaire des paramètres de circulation (modalités d'emprunt et de retour).</summary>
public class ParametreViewModel
{
    [Display(Name = "Quota d'emprunts actifs")]
    [Range(1, 100, ErrorMessage = "Le quota doit être compris entre 1 et 100.")]
    public int QuotaEmpruntsActifs { get; set; } = 3;

    [Display(Name = "Délai de grâce (jours)")]
    [Range(0, 365, ErrorMessage = "Le délai de grâce doit être compris entre 0 et 365 jours.")]
    public int DelaiGraceJours { get; set; }

    [Display(Name = "Plafond de pénalité (€)")]
    [Range(0, 100000, ErrorMessage = "Le plafond doit être positif.")]
    public decimal PlafondPenalite { get; set; }

    [Display(Name = "Bloquer l'emprunt en cas de pénalités impayées")]
    public bool BloquerSiPenalitesImpayees { get; set; } = true;
}
