namespace Application.Services;

/// <summary>Résultat applicatif d'un emprunt réussi.</summary>
public record EmpruntResultat(int EmpruntId, DateTime DateEmprunt, DateTime DateEcheance);

/// <summary>Résultat applicatif d'un retour réussi (avec pénalité éventuelle).</summary>
public record RetourResultat(
    int EmpruntId,
    DateTime DateRetour,
    bool EnRetard,
    int JoursRetard,
    decimal MontantPenalite,
    int? PenaliteId);
