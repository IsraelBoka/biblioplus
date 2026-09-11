using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class ConnexionViewModel
{
    [Required(ErrorMessage = "L'e-mail est requis.")]
    [EmailAddress(ErrorMessage = "E-mail invalide.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string MotDePasse { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
