using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// CRUD MVC du catalogue de livres. Passe par IUnitOfWork (jamais EF Core directement).
/// Les libellés de catégorie et le nombre d'exemplaires sont résolus via des recherches
/// séparées (le repository générique ne charge pas les propriétés de navigation).
/// </summary>
[Authorize(Roles = "Admin")]
public class LivresController : Controller
{
    private readonly IUnitOfWork _uow;

    public LivresController(IUnitOfWork uow) => _uow = uow;

    /// <summary>Construit la liste déroulante des catégories pour les formulaires.</summary>
    private async Task PeuplerCategoriesAsync(int? selectedId, CancellationToken ct)
    {
        var categories = await _uow.CategoriesLivres.ListAsync(ct);
        ViewBag.Categories = new SelectList(
            categories.OrderBy(c => c.Libelle), "Id", "Libelle", selectedId);
    }

    // GET: /Livres
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var livres = await _uow.Livres.ListAsync(ct);
        var categories = (await _uow.CategoriesLivres.ListAsync(ct))
            .ToDictionary(c => c.Id, c => c.Libelle);
        var exemplaires = await _uow.Exemplaires.ListAsync(ct);
        var nbParLivre = exemplaires.GroupBy(e => e.LivreId)
            .ToDictionary(g => g.Key, g => g.Count());

        var vm = livres.Select(l => new LivreViewModel
        {
            Id = l.Id,
            Isbn = l.Isbn,
            Titre = l.Titre,
            Auteur = l.Auteur,
            CategorieLivreId = l.CategorieLivreId,
            CategorieLibelle = categories.TryGetValue(l.CategorieLivreId, out var lib) ? lib : "—",
            NombreExemplaires = nbParLivre.TryGetValue(l.Id, out var n) ? n : 0
        }).ToList();

        return View(vm);
    }

    // GET: /Livres/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var livre = await _uow.Livres.GetByIdAsync(id, ct);
        if (livre is null) return NotFound();

        var categorie = await _uow.CategoriesLivres.GetByIdAsync(livre.CategorieLivreId, ct);
        var exemplaires = await _uow.Exemplaires.ListAsync(ct);

        return View(new LivreViewModel
        {
            Id = livre.Id,
            Isbn = livre.Isbn,
            Titre = livre.Titre,
            Auteur = livre.Auteur,
            CategorieLivreId = livre.CategorieLivreId,
            CategorieLibelle = categorie?.Libelle ?? "—",
            NombreExemplaires = exemplaires.Count(e => e.LivreId == livre.Id)
        });
    }

    // GET: /Livres/Create
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PeuplerCategoriesAsync(null, ct);
        return View(new LivreViewModel());
    }

    // POST: /Livres/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LivreViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PeuplerCategoriesAsync(model.CategorieLivreId, ct);
            return View(model);
        }

        if (await _uow.CategoriesLivres.GetByIdAsync(model.CategorieLivreId, ct) is null)
        {
            ModelState.AddModelError(nameof(model.CategorieLivreId), "Catégorie introuvable.");
            await PeuplerCategoriesAsync(model.CategorieLivreId, ct);
            return View(model);
        }

        await _uow.Livres.AddAsync(new Livre
        {
            Isbn = model.Isbn,
            Titre = model.Titre,
            Auteur = model.Auteur,
            CategorieLivreId = model.CategorieLivreId
        }, ct);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Livre créé.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Livres/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var livre = await _uow.Livres.GetByIdAsync(id, ct);
        if (livre is null) return NotFound();

        await PeuplerCategoriesAsync(livre.CategorieLivreId, ct);
        return View(new LivreViewModel
        {
            Id = livre.Id,
            Isbn = livre.Isbn,
            Titre = livre.Titre,
            Auteur = livre.Auteur,
            CategorieLivreId = livre.CategorieLivreId
        });
    }

    // POST: /Livres/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LivreViewModel model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PeuplerCategoriesAsync(model.CategorieLivreId, ct);
            return View(model);
        }

        var livre = await _uow.Livres.GetByIdAsync(id, ct);
        if (livre is null) return NotFound();

        if (await _uow.CategoriesLivres.GetByIdAsync(model.CategorieLivreId, ct) is null)
        {
            ModelState.AddModelError(nameof(model.CategorieLivreId), "Catégorie introuvable.");
            await PeuplerCategoriesAsync(model.CategorieLivreId, ct);
            return View(model);
        }

        livre.Isbn = model.Isbn;
        livre.Titre = model.Titre;
        livre.Auteur = model.Auteur;
        livre.CategorieLivreId = model.CategorieLivreId;

        _uow.Livres.Update(livre);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Livre modifié.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Livres/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var livre = await _uow.Livres.GetByIdAsync(id, ct);
        if (livre is null) return NotFound();

        var categorie = await _uow.CategoriesLivres.GetByIdAsync(livre.CategorieLivreId, ct);
        return View(new LivreViewModel
        {
            Id = livre.Id,
            Isbn = livre.Isbn,
            Titre = livre.Titre,
            Auteur = livre.Auteur,
            CategorieLivreId = livre.CategorieLivreId,
            CategorieLibelle = categorie?.Libelle ?? "—"
        });
    }

    // POST: /Livres/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var livre = await _uow.Livres.GetByIdAsync(id, ct);
        if (livre is null) return NotFound();

        // Refus si le livre possède encore des exemplaires.
        var exemplaires = await _uow.Exemplaires.ListAsync(ct);
        if (exemplaires.Any(e => e.LivreId == id))
        {
            TempData["Erreur"] = "Suppression refusée : des exemplaires sont rattachés à ce livre.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _uow.Livres.Remove(livre); // suppression logique
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Livre supprimé (suppression logique).";
        return RedirectToAction(nameof(Index));
    }
}
