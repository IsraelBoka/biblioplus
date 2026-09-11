using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// CRUD MVC des adhérents. Passe par IUnitOfWork (jamais EF Core directement).
/// </summary>
[Authorize(Roles = "Admin")]
public class AdherentsController : Controller
{
    private readonly IUnitOfWork _uow;

    public AdherentsController(IUnitOfWork uow) => _uow = uow;

    private static AdherentViewModel ToViewModel(Adherent a) => new()
    {
        Id = a.Id,
        Numero = a.Numero,
        Nom = a.Nom,
        Telephone = a.Telephone,
        DateAdhesion = a.DateAdhesion,
        Actif = a.Actif
    };

    // GET: /Adherents
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var adherents = await _uow.Adherents.ListAsync(ct);
        return View(adherents.Select(ToViewModel).ToList());
    }

    // GET: /Adherents/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var adherent = await _uow.Adherents.GetByIdAsync(id, ct);
        if (adherent is null) return NotFound();
        return View(ToViewModel(adherent));
    }

    // GET: /Adherents/Create
    public IActionResult Create() => View(new AdherentViewModel());

    // POST: /Adherents/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdherentViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _uow.Adherents.NumeroExisteAsync(model.Numero, null, ct))
        {
            ModelState.AddModelError(nameof(model.Numero), "Ce numéro existe déjà.");
            return View(model);
        }

        await _uow.Adherents.AddAsync(new Adherent
        {
            Numero = model.Numero,
            Nom = model.Nom,
            Telephone = model.Telephone,
            DateAdhesion = model.DateAdhesion,
            Actif = model.Actif
        }, ct);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Adhérent créé.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Adherents/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var adherent = await _uow.Adherents.GetByIdAsync(id, ct);
        if (adherent is null) return NotFound();
        return View(ToViewModel(adherent));
    }

    // POST: /Adherents/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdherentViewModel model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var adherent = await _uow.Adherents.GetByIdAsync(id, ct);
        if (adherent is null) return NotFound();

        if (await _uow.Adherents.NumeroExisteAsync(model.Numero, id, ct))
        {
            ModelState.AddModelError(nameof(model.Numero), "Ce numéro existe déjà.");
            return View(model);
        }

        adherent.Numero = model.Numero;
        adherent.Nom = model.Nom;
        adherent.Telephone = model.Telephone;
        adherent.DateAdhesion = model.DateAdhesion;
        adherent.Actif = model.Actif;

        _uow.Adherents.Update(adherent);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Adhérent modifié.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Adherents/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var adherent = await _uow.Adherents.GetByIdAsync(id, ct);
        if (adherent is null) return NotFound();
        return View(ToViewModel(adherent));
    }

    // POST: /Adherents/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var adherent = await _uow.Adherents.GetByIdAsync(id, ct);
        if (adherent is null) return NotFound();

        // Refus si l'adhérent a encore des pénalités non réglées.
        if (await _uow.Penalites.AdherentADesPenalitesNonRegleesAsync(id, ct))
        {
            TempData["Erreur"] = "Suppression refusée : l'adhérent a des pénalités non réglées.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        // Refus s'il reste des emprunts actifs.
        if (await _uow.Emprunts.CompterActifsParAdherentAsync(id, ct) > 0)
        {
            TempData["Erreur"] = "Suppression refusée : l'adhérent a des emprunts actifs.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _uow.Adherents.Remove(adherent); // suppression logique
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Adhérent supprimé (suppression logique).";
        return RedirectToAction(nameof(Index));
    }
}
