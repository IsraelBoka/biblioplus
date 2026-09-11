using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Web.Models;

/// <summary>Modèle de formulaire d'un exemplaire physique (messages en français).</summary>
public class ExemplaireViewModel
{
    public int Id { get; set; }

    [Display(Name = "Code-barres")]
    [Required(ErrorMessage = "Le code-barres est obligatoire.")]
    [MaxLength(50, ErrorMessage = "Le code-barres ne peut pas dépasser 50 caractères.")]
    public string CodeBarres { get; set; } = string.Empty;

    [Display(Name = "État")]
    [MaxLength(100, ErrorMessage = "L'état ne peut pas dépasser 100 caractères.")]
    public string Etat { get; set; } = "Bon";

    [Display(Name = "Statut")]
    public StatutExemplaire Statut { get; set; } = StatutExemplaire.Disponible;

    [Display(Name = "Livre")]
    [Range(1, int.MaxValue, ErrorMessage = "Le livre est obligatoire.")]
    public int LivreId { get; set; }

    /// <summary>Titre du livre (affichage seul).</summary>
    [Display(Name = "Livre")]
    public string? LivreTitre { get; set; }
}
