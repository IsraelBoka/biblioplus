using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Œuvre du catalogue. Un livre appartient à une catégorie et possède
/// un ou plusieurs exemplaires physiques.
/// </summary>
public class Livre : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string Isbn { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Auteur { get; set; } = string.Empty;

    // Relation 1—* : CategorieLivre -> Livre
    public int CategorieLivreId { get; set; }
    public CategorieLivre? CategorieLivre { get; set; }

    // Navigation : un livre possède plusieurs exemplaires.
    public ICollection<Exemplaire> Exemplaires { get; set; } = new List<Exemplaire>();
}
