using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Membre de la bibliothèque autorisé à emprunter des exemplaires.
/// </summary>
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

    public DateTime DateAdhesion { get; set; } = DateTime.UtcNow.Date;

    public bool Actif { get; set; } = true;

    // Navigation : un adhérent réalise plusieurs emprunts et reçoit plusieurs pénalités.
    public ICollection<Emprunt> Emprunts { get; set; } = new List<Emprunt>();
    public ICollection<Penalite> Penalites { get; set; } = new List<Penalite>();
}
