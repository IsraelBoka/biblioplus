using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class InscriptionViewModel
{
    [Required(ErrorMessage = "Le nom est requis.")]
    [MaxLength(150)]
    [Display(Name = "Nom complet")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'e-mail est requis.")]
    [EmailAddress(ErrorMessage = "E-mail invalide.")]
    [MaxLength(150)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [MaxLength(30)]
    [Display(Name = "Téléphone (facultatif)")]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string MotDePasse { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation est requise.")]
    [DataType(DataType.Password)]
    [Compare(nameof(MotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    [Display(Name = "Confirmer le mot de passe")]
    public string Confirmation { get; set; } = string.Empty;
}
