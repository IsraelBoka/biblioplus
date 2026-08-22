using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Transaction de circulation : un adhérent emprunte un exemplaire pour une période.
/// Convention d'intervalle : [DateEmprunt, DateEcheance[.
/// </summary>
public class Emprunt : BaseEntity
{
    // Relation 1—* : Adherent -> Emprunt
    public int AdherentId { get; set; }
    public Adherent? Adherent { get; set; }

    // Relation 1—* : Exemplaire -> Emprunt
    public int ExemplaireId { get; set; }
    public Exemplaire? Exemplaire { get; set; }

    public DateTime DateEmprunt { get; set; }

    /// <summary>Échéance calculée par le serveur : DateEmprunt + DureeMaxJours de la catégorie.</summary>
    public DateTime DateEcheance { get; set; }

    /// <summary>Nulle tant que l'exemplaire n'est pas rendu. Renseignée au retour.</summary>
    public DateTime? DateRetour { get; set; }

    // Relation 1—0..1 : un emprunt peut engendrer une pénalité de retard.
    public Penalite? Penalite { get; set; }

    /// <summary>Un emprunt est actif tant qu'il n'a pas de date de retour.</summary>
    public bool EstActif => DateRetour is null;
}
