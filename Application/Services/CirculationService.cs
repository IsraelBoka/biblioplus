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
    private readonly IUnitOfWork _uow;
    private readonly IEmailSender _email;

    public CirculationService(IUnitOfWork uow, IEmailSender email)
    {
        _uow = uow;
        _email = email;
    }

    public async Task<Result<EmpruntResultat>> EmprunterAsync(
        int adherentId, int exemplaireId, CancellationToken ct = default)
    {
        var parametres = await _uow.Parametres.GetAsync(ct);

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

        // Règle 1 — éligibilité de l'adhérent (modalités paramétrables)
        if (!adherent.Actif)
        {
            return Result<EmpruntResultat>.Conflit("L'adhérent est inactif.");
        }
        if (parametres.BloquerSiPenalitesImpayees
            && await _uow.Penalites.AdherentADesPenalitesNonRegleesAsync(adherentId, ct))
        {
            return Result<EmpruntResultat>.Conflit("L'adhérent a des pénalités non réglées.");
        }
        if (await _uow.Emprunts.CompterActifsParAdherentAsync(adherentId, ct) >= parametres.QuotaEmpruntsActifs)
        {
            var quota = parametres.QuotaEmpruntsActifs;
            var pluriel = quota > 1 ? "s" : "";
            return Result<EmpruntResultat>.Conflit(
                $"L'adhérent a déjà atteint le quota de {quota} emprunt{pluriel} actif{pluriel}.");
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

        // Alerte e-mail (best-effort : n'affecte jamais le résultat de l'emprunt).
        if (!string.IsNullOrWhiteSpace(adherent.Email))
        {
            var titre = exemplaire.Livre?.Titre ?? "un ouvrage";
            await EnvoyerSansEchecAsync(
                adherent.Email!,
                "BiblioPlus — Confirmation d'emprunt",
                $"<p>Bonjour {adherent.Nom},</p>" +
                $"<p>Votre emprunt de « <strong>{titre}</strong> » est confirmé.</p>" +
                $"<p>À rendre avant le <strong>{dateEcheance:dd/MM/yyyy}</strong>.</p>" +
                "<p>— L'équipe BiblioPlus</p>",
                ct);
        }

        return Result<EmpruntResultat>.Succes(
            new EmpruntResultat(emprunt.Id, emprunt.DateEmprunt, emprunt.DateEcheance));
    }

    public async Task<Result<RetourResultat>> RetournerAsync(int empruntId, CancellationToken ct = default)
    {
        var parametres = await _uow.Parametres.GetAsync(ct);

        var emprunt = await _uow.Emprunts.GetActifAvecCategorieAsync(empruntId, ct);
        if (emprunt is null)
        {
            return Result<RetourResultat>.Introuvable("Emprunt actif introuvable (ou déjà retourné).");
        }

        var exemplaire = emprunt.Exemplaire!;
        var categorie = exemplaire.Livre!.CategorieLivre!;

        var dateRetour = DateTime.UtcNow.Date;
        emprunt.DateRetour = dateRetour;

        // Règle 4 — pénalité si retard, puis libération de l'exemplaire (atomique).
        // Modalités paramétrables : délai de grâce (jours facturés au-delà de la tolérance)
        // et plafond de pénalité par emprunt.
        var joursRetard = 0;
        decimal montant = 0m;
        int? penaliteId = null;

        var dateLimiteAvecGrace = emprunt.DateEcheance.Date.AddDays(parametres.DelaiGraceJours);
        if (dateRetour > dateLimiteAvecGrace)
        {
            joursRetard = (dateRetour - dateLimiteAvecGrace).Days; // jours facturés après tolérance
            montant = joursRetard * categorie.PenaliteParJour;

            if (parametres.PlafondPenalite > 0m && montant > parametres.PlafondPenalite)
            {
                montant = parametres.PlafondPenalite; // application du plafond
            }

            var motif = parametres.DelaiGraceJours > 0
                ? $"Retard de {joursRetard} jour(s) facturé(s) (tolérance de {parametres.DelaiGraceJours} j) sur l'emprunt #{emprunt.Id}."
                : $"Retard de {joursRetard} jour(s) sur l'emprunt #{emprunt.Id}.";

            var penalite = new Penalite
            {
                AdherentId = emprunt.AdherentId,
                EmpruntId = emprunt.Id,
                Motif = motif,
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

        // Alerte e-mail en cas de pénalité de retard (best-effort).
        if (joursRetard > 0)
        {
            var adherent = await _uow.Adherents.GetByIdAsync(emprunt.AdherentId, ct);
            if (adherent is not null && !string.IsNullOrWhiteSpace(adherent.Email))
            {
                await EnvoyerSansEchecAsync(
                    adherent.Email!,
                    "BiblioPlus — Pénalité de retard",
                    $"<p>Bonjour {adherent.Nom},</p>" +
                    $"<p>Votre retour de l'emprunt #{emprunt.Id} accuse un retard de " +
                    $"<strong>{joursRetard} jour(s)</strong>.</p>" +
                    $"<p>Une pénalité de <strong>{montant:0.00} €</strong> a été enregistrée.</p>" +
                    "<p>— L'équipe BiblioPlus</p>",
                    ct);
            }
        }

        return Result<RetourResultat>.Succes(
            new RetourResultat(emprunt.Id, dateRetour, joursRetard > 0, joursRetard, montant, penaliteId));
    }

    /// <summary>Envoi d'alerte tolérant aux pannes : une erreur SMTP ne compromet pas la circulation.</summary>
    private async Task EnvoyerSansEchecAsync(string destinataire, string sujet, string corps, CancellationToken ct)
    {
        try
        {
            await _email.SendAsync(destinataire, sujet, corps, ct);
        }
        catch
        {
            // Volontairement ignoré : la règle métier a déjà été persistée.
        }
    }
}
