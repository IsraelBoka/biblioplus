using System.ComponentModel.DataAnnotations;

namespace Web.Models;

/// <summary>Modèle de formulaire d'un adhérent (messages en français).</summary>
public class AdherentViewModel
{
    public int Id { get; set; }

    [Display(Name = "Numéro")]
    [Required(ErrorMessage = "Le numéro est obligatoire.")]
    [MaxLength(20, ErrorMessage = "Le numéro ne peut pas dépasser 20 caractères.")]
    public string Numero { get; set; } = string.Empty;

    [Display(Name = "Nom")]
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [MaxLength(150, ErrorMessage = "Le nom ne peut pas dépasser 150 caractères.")]
    public string Nom { get; set; } = string.Empty;

    [Display(Name = "Téléphone")]
    [MaxLength(30, ErrorMessage = "Le téléphone ne peut pas dépasser 30 caractères.")]
    public string? Telephone { get; set; }

    [Display(Name = "Date d'adhésion")]
    [DataType(DataType.Date)]
    public DateTime DateAdhesion { get; set; } = DateTime.UtcNow.Date;

    [Display(Name = "Actif")]
    public bool Actif { get; set; } = true;
}
