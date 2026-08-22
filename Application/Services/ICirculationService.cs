using Application.Common;

namespace Application.Services;

/// <summary>
/// Service du workflow principal de circulation, de l'emprunt au retour.
/// Ne dépend ni de MVC, ni de ControllerBase, ni des vues Razor.
/// </summary>
public interface ICirculationService
{
    /// <summary>Crée un emprunt après vérification des règles 1 à 3.</summary>
    Task<Result<EmpruntResultat>> EmprunterAsync(int adherentId, int exemplaireId, CancellationToken ct = default);

    /// <summary>Enregistre un retour, applique la règle 4 (pénalité) et libère l'exemplaire.</summary>
    Task<Result<RetourResultat>> RetournerAsync(int empruntId, CancellationToken ct = default);
}
