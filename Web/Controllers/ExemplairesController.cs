using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// CRUD MVC des exemplaires physiques. Passe par IUnitOfWork (jamais EF Core directement).
/// </summary>
[Authorize(Roles = "Admin")]
public class ExemplairesController : Controller
{
    private readonly IUnitOfWork _uow;

    public ExemplairesController(IUnitOfWork uow) => _uow = uow;

    /// <summary>Construit la liste déroulante des livres pour les formulaires.</summary>
    private async Task PeuplerLivresAsync(int? selectedId, CancellationToken ct)
    {
        var livres = await _uow.Livres.ListAsync(ct);
        ViewBag.Livres = new SelectList(
            livres.OrderBy(l => l.Titre), "Id", "Titre", selectedId);
    }

    // GET: /Exemplaires
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var exemplaires = await _uow.Exemplaires.ListAsync(ct);
        var livres = (await _uow.Livres.ListAsync(ct))
            .ToDictionary(l => l.Id, l => l.Titre);

        var vm = exemplaires.Select(e => new ExemplaireViewModel
        {
            Id = e.Id,
            CodeBarres = e.CodeBarres,
            Etat = e.Etat,
            Statut = e.Statut,
            LivreId = e.LivreId,
            LivreTitre = livres.TryGetValue(e.LivreId, out var t) ? t : "—"
        }).ToList();

        return View(vm);
    }

    // GET: /Exemplaires/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var exemplaire = await _uow.Exemplaires.GetByIdAsync(id, ct);
        if (exemplaire is null) return NotFound();

        var livre = await _uow.Livres.GetByIdAsync(exemplaire.LivreId, ct);
        return View(ToViewModel(exemplaire, livre?.Titre));
    }

    // GET: /Exemplaires/Create
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PeuplerLivresAsync(null, ct);
        return View(new ExemplaireViewModel());
    }

    // POST: /Exemplaires/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExemplaireViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PeuplerLivresAsync(model.LivreId, ct);
            return View(model);
        }

        if (await _uow.Livres.GetByIdAsync(model.LivreId, ct) is null)
        {
            ModelState.AddModelError(nameof(model.LivreId), "Livre introuvable.");
            await PeuplerLivresAsync(model.LivreId, ct);
            return View(model);
        }

        await _uow.Exemplaires.AddAsync(new Exemplaire
        {
            CodeBarres = model.CodeBarres,
            Etat = model.Etat,
            Statut = model.Statut,
            LivreId = model.LivreId
        }, ct);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Exemplaire créé.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Exemplaires/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var exemplaire = await _uow.Exemplaires.GetByIdAsync(id, ct);
        if (exemplaire is null) return NotFound();

        await PeuplerLivresAsync(exemplaire.LivreId, ct);
        return View(ToViewModel(exemplaire, null));
    }

    // POST: /Exemplaires/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExemplaireViewModel model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PeuplerLivresAsync(model.LivreId, ct);
            return View(model);
        }

        var exemplaire = await _uow.Exemplaires.GetByIdAsync(id, ct);
        if (exemplaire is null) return NotFound();

        if (await _uow.Livres.GetByIdAsync(model.LivreId, ct) is null)
        {
            ModelState.AddModelError(nameof(model.LivreId), "Livre introuvable.");
            await PeuplerLivresAsync(model.LivreId, ct);
            return View(model);
        }

        exemplaire.CodeBarres = model.CodeBarres;
        exemplaire.Etat = model.Etat;
        exemplaire.Statut = model.Statut;
        exemplaire.LivreId = model.LivreId;

        _uow.Exemplaires.Update(exemplaire);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Exemplaire modifié.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Exemplaires/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var exemplaire = await _uow.Exemplaires.GetByIdAsync(id, ct);
        if (exemplaire is null) return NotFound();

        var livre = await _uow.Livres.GetByIdAsync(exemplaire.LivreId, ct);
        return View(ToViewModel(exemplaire, livre?.Titre));
    }

    // POST: /Exemplaires/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var exemplaire = await _uow.Exemplaires.GetByIdAsync(id, ct);
        if (exemplaire is null) return NotFound();

        // Refus si l'exemplaire est actuellement emprunté.
        if (exemplaire.Statut == StatutExemplaire.Emprunte)
        {
            TempData["Erreur"] = "Suppression refusée : l'exemplaire est actuellement emprunté.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _uow.Exemplaires.Remove(exemplaire); // suppression logique
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Exemplaire supprimé (suppression logique).";
        return RedirectToAction(nameof(Index));
    }

    private static ExemplaireViewModel ToViewModel(Exemplaire e, string? livreTitre) => new()
    {
        Id = e.Id,
        CodeBarres = e.CodeBarres,
        Etat = e.Etat,
        Statut = e.Statut,
        LivreId = e.LivreId,
        LivreTitre = livreTitre
    };
}
