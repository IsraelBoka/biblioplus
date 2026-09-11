using System.ComponentModel.DataAnnotations;

namespace Api.Contracts;

public record LivreDisponibleResponse(
    int Id,
    string Isbn,
    string Titre,
    string Auteur,
    string Categorie,
    int ExemplairesDisponibles);

public class CreerEmpruntRequest
{
    [Range(1, int.MaxValue)]
    public int AdherentId { get; set; }

    [Range(1, int.MaxValue)]
    public int ExemplaireId { get; set; }
}

public record EmpruntResponse(int EmpruntId, DateTime DateEmprunt, DateTime DateEcheance);

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
