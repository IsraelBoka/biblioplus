using Application.Abstractions;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// Écran de circulation : liste des emprunts, création (emprunt) et retour.
/// La logique métier (règles 1 à 4) reste dans <see cref="ICirculationService"/> ;
/// le contrôleur ne fait qu'orchestrer l'IHM et lire les données via IUnitOfWork.
/// </summary>
[Authorize(Roles = "Admin")]
public class EmpruntsController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly ICirculationService _circulation;

    public EmpruntsController(IUnitOfWork uow, ICirculationService circulation)
    {
        _uow = uow;
        _circulation = circulation;
    }

    // GET: /Emprunts
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var emprunts = await _uow.Emprunts.ListAsync(ct);
        var adherents = (await _uow.Adherents.ListAsync(ct))
            .ToDictionary(a => a.Id, a => a.Nom);
        var exemplaires = (await _uow.Exemplaires.ListAsync(ct))
            .ToDictionary(e => e.Id, e => e);
        var livres = (await _uow.Livres.ListAsync(ct))
            .ToDictionary(l => l.Id, l => l.Titre);

        var vm = emprunts
            .OrderByDescending(e => e.EstActif)
            .ThenByDescending(e => e.DateEmprunt)
            .Select(e =>
            {
                exemplaires.TryGetValue(e.ExemplaireId, out var ex);
                var titre = ex is not null && livres.TryGetValue(ex.LivreId, out var t) ? t : "—";
                return new EmpruntViewModel
                {
                    Id = e.Id,
                    AdherentNom = adherents.TryGetValue(e.AdherentId, out var nom) ? nom : "—",
                    ExemplaireCodeBarres = ex?.CodeBarres ?? "—",
                    LivreTitre = titre,
                    DateEmprunt = e.DateEmprunt,
                    DateEcheance = e.DateEcheance,
                    DateRetour = e.DateRetour
                };
            }).ToList();

        return View(vm);
    }

    // GET: /Emprunts/Create
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PeuplerListesAsync(ct);
        return View(new EmpruntCreateViewModel());
    }

    // POST: /Emprunts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmpruntCreateViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PeuplerListesAsync(ct);
            return View(model);
        }

        var resultat = await _circulation.EmprunterAsync(model.AdherentId, model.ExemplaireId, ct);
        if (!resultat.EstSucces)
        {
            ModelState.AddModelError(string.Empty, resultat.Erreur ?? "Emprunt refusé.");
            await PeuplerListesAsync(ct);
            return View(model);
        }

        var r = resultat.Valeur!;
        TempData["Message"] =
            $"Emprunt #{r.EmpruntId} enregistré. À rendre avant le {r.DateEcheance:dd/MM/yyyy}.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Emprunts/Retourner/5
    [HttpPost, ActionName("Retourner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retourner(int id, CancellationToken ct)
    {
        var resultat = await _circulation.RetournerAsync(id, ct);
        if (!resultat.EstSucces)
        {
            TempData["Erreur"] = resultat.Erreur ?? "Retour refusé.";
            return RedirectToAction(nameof(Index));
        }

        var r = resultat.Valeur!;
        TempData["Message"] = r.EnRetard
            ? $"Retour enregistré avec {r.JoursRetard} jour(s) de retard — pénalité de {r.MontantPenalite:0.00}."
            : "Retour enregistré. Aucun retard.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Adhérents actifs + exemplaires disponibles pour le formulaire d'emprunt.</summary>
    private async Task PeuplerListesAsync(CancellationToken ct)
    {
        var adherents = (await _uow.Adherents.ListAsync(ct))
            .Where(a => a.Actif)
            .OrderBy(a => a.Nom)
            .Select(a => new { a.Id, Libelle = $"{a.Nom} ({a.Numero})" });
        ViewBag.Adherents = new SelectList(adherents, "Id", "Libelle");

        var livres = (await _uow.Livres.ListAsync(ct)).ToDictionary(l => l.Id, l => l.Titre);
        var exemplaires = (await _uow.Exemplaires.ListAsync(ct))
            .Where(e => e.Statut == StatutExemplaire.Disponible)
            .OrderBy(e => e.CodeBarres)
            .Select(e => new
            {
                e.Id,
                Libelle = $"{e.CodeBarres} — {(livres.TryGetValue(e.LivreId, out var t) ? t : "?")}"
            });
        ViewBag.Exemplaires = new SelectList(exemplaires, "Id", "Libelle");
    }
}
