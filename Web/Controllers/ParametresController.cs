using Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// Partie « Paramètres » : édition des modalités globales de circulation
/// (quota d'emprunts, délai de grâce, plafond de pénalité, blocage). Passe par
/// IUnitOfWork ; les valeurs sont ensuite lues par <c>CirculationService</c>.
/// </summary>
[Authorize(Roles = "Admin")]
public class ParametresController : Controller
{
    private readonly IUnitOfWork _uow;

    public ParametresController(IUnitOfWork uow) => _uow = uow;

    // GET: /Parametres
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var p = await _uow.Parametres.GetAsync(ct);
        return View(new ParametreViewModel
        {
            QuotaEmpruntsActifs = p.QuotaEmpruntsActifs,
            DelaiGraceJours = p.DelaiGraceJours,
            PlafondPenalite = p.PlafondPenalite,
            BloquerSiPenalitesImpayees = p.BloquerSiPenalitesImpayees
        });
    }

    // POST: /Parametres
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ParametreViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var p = await _uow.Parametres.GetAsync(ct);
        p.QuotaEmpruntsActifs = model.QuotaEmpruntsActifs;
        p.DelaiGraceJours = model.DelaiGraceJours;
        p.PlafondPenalite = model.PlafondPenalite;
        p.BloquerSiPenalitesImpayees = model.BloquerSiPenalitesImpayees;

        _uow.Parametres.Update(p);
        await _uow.SaveChangesAsync(ct);

        TempData["Message"] = "Paramètres enregistrés. Ils s'appliquent aux prochains emprunts et retours.";
        return RedirectToAction(nameof(Index));
    }
}
