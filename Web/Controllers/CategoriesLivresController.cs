using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// CRUD MVC du référentiel CategorieLivre. Passe par IUnitOfWork (jamais EF Core directement).
/// </summary>
[Authorize(Roles = "Admin")]
public class CategoriesLivresController : Controller
{
    private readonly IUnitOfWork _uow;

    public CategoriesLivresController(IUnitOfWork uow) => _uow = uow;

    private static CategorieLivreViewModel ToViewModel(CategorieLivre c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Libelle = c.Libelle,
        DureeMaxJours = c.DureeMaxJours,
        PenaliteParJour = c.PenaliteParJour
    };

    // GET: /CategoriesLivres
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var categories = await _uow.CategoriesLivres.ListAsync(ct);
        return View(categories.Select(ToViewModel).ToList());
    }

    // GET: /CategoriesLivres/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var categorie = await _uow.CategoriesLivres.GetByIdAsync(id, ct);
        if (categorie is null) return NotFound();
        return View(ToViewModel(categorie));
    }

    // GET: /CategoriesLivres/Create
    public IActionResult Create() => View(new CategorieLivreViewModel());

    // POST: /CategoriesLivres/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategorieLivreViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _uow.CategoriesLivres.CodeExisteAsync(model.Code, null, ct))
        {
            ModelState.AddModelError(nameof(model.Code), "Ce code existe déjà.");
            return View(model);
        }

        await _uow.CategoriesLivres.AddAsync(new CategorieLivre
        {
            Code = model.Code,
            Libelle = model.Libelle,
            DureeMaxJours = model.DureeMaxJours,
            PenaliteParJour = model.PenaliteParJour
        }, ct);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Catégorie créée.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /CategoriesLivres/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var categorie = await _uow.CategoriesLivres.GetByIdAsync(id, ct);
        if (categorie is null) return NotFound();
        return View(ToViewModel(categorie));
    }

    // POST: /CategoriesLivres/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategorieLivreViewModel model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var categorie = await _uow.CategoriesLivres.GetByIdAsync(id, ct);
        if (categorie is null) return NotFound();

        if (await _uow.CategoriesLivres.CodeExisteAsync(model.Code, id, ct))
        {
            ModelState.AddModelError(nameof(model.Code), "Ce code existe déjà.");
            return View(model);
        }

        categorie.Code = model.Code;
        categorie.Libelle = model.Libelle;
        categorie.DureeMaxJours = model.DureeMaxJours;
        categorie.PenaliteParJour = model.PenaliteParJour;

        _uow.CategoriesLivres.Update(categorie);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Catégorie modifiée.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /CategoriesLivres/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var categorie = await _uow.CategoriesLivres.GetByIdAsync(id, ct);
        if (categorie is null) return NotFound();
        return View(ToViewModel(categorie));
    }

    // POST: /CategoriesLivres/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var categorie = await _uow.CategoriesLivres.GetByIdAsync(id, ct);
        if (categorie is null) return NotFound();

        // Refus si le référentiel est encore utilisé par une donnée métier active.
        if (await _uow.CategoriesLivres.ADesLivresActifsAsync(id, ct))
        {
            TempData["Erreur"] = "Suppression refusée : des livres actifs utilisent cette catégorie.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _uow.CategoriesLivres.Remove(categorie); // suppression logique
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Catégorie supprimée (suppression logique).";
        return RedirectToAction(nameof(Index));
    }
}
