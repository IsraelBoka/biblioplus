using Application.Abstractions;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// Espace personnel de l'adhérent : tableau de bord, catalogue, ses emprunts, ses pénalités.
/// Lecture seule — la circulation reste gérée par l'espace admin.
/// </summary>
[Authorize(Roles = "Adherent")]
public class EspaceController : Controller
{
    private readonly IUnitOfWork _uow;

    public EspaceController(IUnitOfWork uow) => _uow = uow;

    // GET: /Espace
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var adherentId = AdherentIdCourant();
        if (adherentId is null) return CompteNonRattache();

        var adherent = await _uow.Adherents.GetByIdAsync(adherentId.Value, ct);
        if (adherent is null) return CompteNonRattache();

        var emprunts = await _uow.Emprunts.ListParAdherentAsync(adherentId.Value, ct);
        var penalites = await _uow.Penalites.ListParAdherentAsync(adherentId.Value, ct);
        var aujourdHui = DateTime.UtcNow.Date;
        var aRegler = penalites.Where(p => p.Etat == EtatPenalite.ARegler).ToList();

        var vm = new EspaceDashboardViewModel
        {
            Nom = adherent.Nom,
            Numero = adherent.Numero,
            EmpruntsActifs = emprunts.Count(e => e.EstActif),
            EmpruntsEnRetard = emprunts.Count(e => e.EstActif && aujourdHui > e.DateEcheance.Date),
            PenalitesARegler = aRegler.Count,
            MontantDu = aRegler.Sum(p => p.Montant),
            DerniersEmprunts = emprunts.Take(6).Select(e => new EspaceEmpruntLigne
            {
                Id = e.Id,
                LivreTitre = e.Exemplaire?.Livre?.Titre ?? "—",
                DateEmprunt = e.DateEmprunt,
                DateEcheance = e.DateEcheance,
                DateRetour = e.DateRetour,
                EnRetard = e.EstActif && aujourdHui > e.DateEcheance.Date
            }).ToList()
        };

        return View(vm);
    }

    // GET: /Espace/Catalogue
    public async Task<IActionResult> Catalogue(string? recherche, CancellationToken ct)
    {
        var livres = await _uow.Livres.GetDisponiblesAsync(recherche, ct);
        ViewData["Recherche"] = recherche;

        var lignes = livres.Select(l => new EspaceCatalogueLigne
        {
            Titre = l.Titre,
            Auteur = l.Auteur,
            Isbn = l.Isbn,
            Categorie = l.CategorieLivre?.Libelle ?? "—",
            ExemplairesDisponibles = l.Exemplaires.Count(e => e.Statut == StatutExemplaire.Disponible)
        }).ToList();

        return View(lignes);
    }

    // GET: /Espace/MesEmprunts
    public async Task<IActionResult> MesEmprunts(CancellationToken ct)
    {
        var adherentId = AdherentIdCourant();
        if (adherentId is null) return CompteNonRattache();

        var aujourdHui = DateTime.UtcNow.Date;
        var emprunts = await _uow.Emprunts.ListParAdherentAsync(adherentId.Value, ct);

        var lignes = emprunts.Select(e => new EspaceEmpruntLigne
        {
            Id = e.Id,
            LivreTitre = e.Exemplaire?.Livre?.Titre ?? "—",
            DateEmprunt = e.DateEmprunt,
            DateEcheance = e.DateEcheance,
            DateRetour = e.DateRetour,
            EnRetard = e.EstActif && aujourdHui > e.DateEcheance.Date
        }).ToList();

        return View(lignes);
    }

    // GET: /Espace/MesPenalites
    public async Task<IActionResult> MesPenalites(CancellationToken ct)
    {
        var adherentId = AdherentIdCourant();
        if (adherentId is null) return CompteNonRattache();

        var penalites = await _uow.Penalites.ListParAdherentAsync(adherentId.Value, ct);
        var lignes = penalites.Select(p => new EspacePenaliteLigne
        {
            Id = p.Id,
            Motif = p.Motif,
            Montant = p.Montant,
            Etat = p.Etat,
            Date = p.CreatedAt
        }).ToList();

        return View(lignes);
    }

    private int? AdherentIdCourant()
        => int.TryParse(User.FindFirst(CompteController.ClaimAdherentId)?.Value, out var id) ? id : null;

    private IActionResult CompteNonRattache()
    {
        TempData["Erreur"] = "Votre compte n'est rattaché à aucun adhérent. Contactez la bibliothèque.";
        return View("Index", new EspaceDashboardViewModel());
    }
}
