using Application.Abstractions;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// Suivi des pénalités : consultation et changement d'état (réglée / annulée).
/// Les pénalités sont créées automatiquement par le retour d'un emprunt en retard.
/// </summary>
[Authorize(Roles = "Admin")]
public class PenalitesController : Controller
{
    private readonly IUnitOfWork _uow;

    public PenalitesController(IUnitOfWork uow) => _uow = uow;

    // GET: /Penalites
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var penalites = await _uow.Penalites.ListAsync(ct);
        var adherents = (await _uow.Adherents.ListAsync(ct))
            .ToDictionary(a => a.Id, a => a.Nom);

        var vm = penalites
            .OrderBy(p => p.Etat)
            .ThenByDescending(p => p.Id)
            .Select(p => new PenaliteViewModel
            {
                Id = p.Id,
                AdherentNom = adherents.TryGetValue(p.AdherentId, out var nom) ? nom : "—",
                EmpruntId = p.EmpruntId,
                Motif = p.Motif,
                Montant = p.Montant,
                Etat = p.Etat
            }).ToList();

        return View(vm);
    }

    // POST: /Penalites/Regler/5
    [HttpPost, ActionName("Regler")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Regler(int id, CancellationToken ct)
        => await ChangerEtatAsync(id, EtatPenalite.Reglee, "Pénalité réglée.", ct);

    // POST: /Penalites/Annuler/5
    [HttpPost, ActionName("Annuler")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Annuler(int id, CancellationToken ct)
        => await ChangerEtatAsync(id, EtatPenalite.Annulee, "Pénalité annulée.", ct);

    private async Task<IActionResult> ChangerEtatAsync(
        int id, EtatPenalite etat, string message, CancellationToken ct)
    {
        var penalite = await _uow.Penalites.GetByIdAsync(id, ct);
        if (penalite is null) return NotFound();

        if (penalite.Etat != EtatPenalite.ARegler)
        {
            TempData["Erreur"] = "Cette pénalité est déjà clôturée.";
            return RedirectToAction(nameof(Index));
        }

        penalite.Etat = etat;
        _uow.Penalites.Update(penalite);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = message;
        return RedirectToAction(nameof(Index));
    }
}
