using System.ComponentModel.DataAnnotations;

namespace Web.Models;

/// <summary>Modèle de formulaire du référentiel CategorieLivre (messages en français).</summary>
public class CategorieLivreViewModel
{
    public int Id { get; set; }

    [Display(Name = "Code")]
    [Required(ErrorMessage = "Le code est obligatoire.")]
    [MaxLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères.")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Libellé")]
    [Required(ErrorMessage = "Le libellé est obligatoire.")]
    [MaxLength(100, ErrorMessage = "Le libellé ne peut pas dépasser 100 caractères.")]
    public string Libelle { get; set; } = string.Empty;

    [Display(Name = "Durée max (jours)")]
    [Range(1, 365, ErrorMessage = "La durée doit être comprise entre 1 et 365 jours.")]
    public int DureeMaxJours { get; set; }

    [Display(Name = "Pénalité / jour")]
    [Range(0, 10000, ErrorMessage = "La pénalité journalière doit être positive.")]
    public decimal PenaliteParJour { get; set; }
}
