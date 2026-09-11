using System.ComponentModel.DataAnnotations;

namespace Web.Models;

/// <summary>Modèle de formulaire d'un livre du catalogue (messages en français).</summary>
public class LivreViewModel
{
    public int Id { get; set; }

    [Display(Name = "ISBN")]
    [Required(ErrorMessage = "L'ISBN est obligatoire.")]
    [MaxLength(20, ErrorMessage = "L'ISBN ne peut pas dépasser 20 caractères.")]
    public string Isbn { get; set; } = string.Empty;

    [Display(Name = "Titre")]
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [MaxLength(200, ErrorMessage = "Le titre ne peut pas dépasser 200 caractères.")]
    public string Titre { get; set; } = string.Empty;

    [Display(Name = "Auteur")]
    [Required(ErrorMessage = "L'auteur est obligatoire.")]
    [MaxLength(150, ErrorMessage = "L'auteur ne peut pas dépasser 150 caractères.")]
    public string Auteur { get; set; } = string.Empty;

    [Display(Name = "Catégorie")]
    [Range(1, int.MaxValue, ErrorMessage = "La catégorie est obligatoire.")]
    public int CategorieLivreId { get; set; }

    /// <summary>Libellé de la catégorie (affichage seul).</summary>
    [Display(Name = "Catégorie")]
    public string? CategorieLibelle { get; set; }

    /// <summary>Nombre d'exemplaires rattachés (affichage seul).</summary>
    [Display(Name = "Exemplaires")]
    public int NombreExemplaires { get; set; }
}
