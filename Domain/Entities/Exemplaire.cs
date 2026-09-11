using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Exemplaire : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string CodeBarres { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Etat { get; set; } = "Bon";

    public StatutExemplaire Statut { get; set; } = StatutExemplaire.Disponible;

    public int LivreId { get; set; }
    public Livre? Livre { get; set; }

    public ICollection<Emprunt> Emprunts { get; set; } = new List<Emprunt>();
}
