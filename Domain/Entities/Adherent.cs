using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public class Adherent : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Telephone { get; set; }

    [MaxLength(150)]
    [EmailAddress]
    public string? Email { get; set; }

    public DateTime DateAdhesion { get; set; } = DateTime.UtcNow.Date;

    public bool Actif { get; set; } = true;

    public ICollection<Emprunt> Emprunts { get; set; } = new List<Emprunt>();
    public ICollection<Penalite> Penalites { get; set; } = new List<Penalite>();
}
