using Application.Abstractions;
using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Implémente le workflow de circulation. Reçoit ses dépendances par injection (IUnitOfWork).
/// Chaque cas d'usage réussi appelle SaveChangesAsync une seule fois (atomicité).
/// Convention d'intervalle des périodes : [DateEmprunt, DateEcheance[.
/// </summary>
public class CirculationService : ICirculationService
{
    private const int MaxEmpruntsActifs = 3;

    private readonly IUnitOfWork _uow;

    public CirculationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<EmpruntResultat>> EmprunterAsync(
        int adherentId, int exemplaireId, CancellationToken ct = default)
    {
        var adherent = await _uow.Adherents.GetByIdAsync(adherentId, ct);
        if (adherent is null)
        {
            return Result<EmpruntResultat>.Introuvable("Adhérent introuvable.");
        }

        var exemplaire = await _uow.Exemplaires.GetAvecLivreEtCategorieAsync(exemplaireId, ct);
        if (exemplaire is null)
        {
            return Result<EmpruntResultat>.Introuvable("Exemplaire introuvable.");
        }

        // Règle 1 — éligibilité de l'adhérent
        if (!adherent.Actif)
        {
            return Result<EmpruntResultat>.Conflit("L'adhérent est inactif.");
        }
        if (await _uow.Penalites.AdherentADesPenalitesNonRegleesAsync(adherentId, ct))
        {
            return Result<EmpruntResultat>.Conflit("L'adhérent a des pénalités non réglées.");
        }
        if (await _uow.Emprunts.CompterActifsParAdherentAsync(adherentId, ct) >= MaxEmpruntsActifs)
        {
            return Result<EmpruntResultat>.Conflit(
                $"L'adhérent a déjà {MaxEmpruntsActifs} emprunts actifs.");
        }

        // Règle 2 — disponibilité de l'exemplaire
        if (exemplaire.Statut != StatutExemplaire.Disponible)
        {
            return Result<EmpruntResultat>.Conflit("L'exemplaire n'est pas disponible.");
        }

        // Règle 3 — échéance calculée par le serveur
        var categorie = exemplaire.Livre!.CategorieLivre!;
        var dateEmprunt = DateTime.UtcNow.Date;
        var dateEcheance = dateEmprunt.AddDays(categorie.DureeMaxJours);

        var emprunt = new Emprunt
        {
            AdherentId = adherentId,
            ExemplaireId = exemplaireId,
            DateEmprunt = dateEmprunt,
            DateEcheance = dateEcheance
        };

        exemplaire.Statut = StatutExemplaire.Emprunte;
        _uow.Exemplaires.Update(exemplaire);
        await _uow.Emprunts.AddAsync(emprunt, ct);

        await _uow.SaveChangesAsync(ct); // une seule sauvegarde

        return Result<EmpruntResultat>.Succes(
            new EmpruntResultat(emprunt.Id, emprunt.DateEmprunt, emprunt.DateEcheance));
    }

    public async Task<Result<RetourResultat>> RetournerAsync(int empruntId, CancellationToken ct = default)
    {
        var emprunt = await _uow.Emprunts.GetActifAvecCategorieAsync(empruntId, ct);
        if (emprunt is null)
        {
            return Result<RetourResultat>.Introuvable("Emprunt actif introuvable (ou déjà retourné).");
        }

        var exemplaire = emprunt.Exemplaire!;
        var categorie = exemplaire.Livre!.CategorieLivre!;

        var dateRetour = DateTime.UtcNow.Date;
        emprunt.DateRetour = dateRetour;

        // Règle 4 — pénalité si retard, puis libération de l'exemplaire (atomique)
        var joursRetard = 0;
        decimal montant = 0m;
        int? penaliteId = null;

        if (dateRetour > emprunt.DateEcheance.Date)
        {
            joursRetard = (dateRetour - emprunt.DateEcheance.Date).Days; // jours commencés
            montant = joursRetard * categorie.PenaliteParJour;

            var penalite = new Penalite
            {
                AdherentId = emprunt.AdherentId,
                EmpruntId = emprunt.Id,
                Motif = $"Retard de {joursRetard} jour(s) sur l'emprunt #{emprunt.Id}.",
                Montant = montant,
                Etat = EtatPenalite.ARegler
            };
            await _uow.Penalites.AddAsync(penalite, ct);
            emprunt.Penalite = penalite;
        }

        exemplaire.Statut = StatutExemplaire.Disponible; // le retour libère l'exemplaire
        _uow.Exemplaires.Update(exemplaire);
        _uow.Emprunts.Update(emprunt);

        await _uow.SaveChangesAsync(ct); // une seule sauvegarde

        penaliteId = emprunt.Penalite?.Id;
        return Result<RetourResultat>.Succes(
            new RetourResultat(emprunt.Id, dateRetour, joursRetard > 0, joursRetard, montant, penaliteId));
    }
}
