using System.ComponentModel.DataAnnotations;

namespace Api.Contracts;

/// <summary>Livre proposé par la route de disponibilité.</summary>
public record LivreDisponibleResponse(
    int Id,
    string Isbn,
    string Titre,
    string Auteur,
    string Categorie,
    int ExemplairesDisponibles);

/// <summary>Requête de création d'un emprunt.</summary>
public class CreerEmpruntRequest
{
    [Range(1, int.MaxValue)]
    public int AdherentId { get; set; }

    [Range(1, int.MaxValue)]
    public int ExemplaireId { get; set; }
}

/// <summary>Réponse d'un emprunt créé.</summary>
public record EmpruntResponse(int EmpruntId, DateTime DateEmprunt, DateTime DateEcheance);

/// <summary>Réponse d'un retour (pénalité éventuelle).</summary>
public record RetourResponse(
    int EmpruntId,
    DateTime DateRetour,
    bool EnRetard,
    int JoursRetard,
    decimal MontantPenalite,
    int? PenaliteId);

/// <summary>Réponse d'une pénalité.</summary>
public record PenaliteResponse(
    int Id,
    int AdherentId,
    int EmpruntId,
    string Motif,
    decimal Montant,
    string Etat);
