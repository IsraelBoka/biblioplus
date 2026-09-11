using System.Diagnostics;
using Application.Abstractions;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _uow;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var livres = await _uow.Livres.ListAsync(ct);
        var exemplaires = await _uow.Exemplaires.ListAsync(ct);
        var adherents = await _uow.Adherents.ListAsync(ct);
        var emprunts = await _uow.Emprunts.ListAsync(ct);
        var penalites = await _uow.Penalites.ListAsync(ct);

        var aujourdHui = DateTime.UtcNow.Date;
        var empruntsActifs = emprunts.Where(e => e.DateRetour is null).ToList();
        var penalitesARegler = penalites.Where(p => p.Etat == EtatPenalite.ARegler).ToList();

        var adherentsNoms = adherents.ToDictionary(a => a.Id, a => a.Nom);
        var exemplairesParId = exemplaires.ToDictionary(e => e.Id, e => e);
        var livresTitres = livres.ToDictionary(l => l.Id, l => l.Titre);

        var vm = new DashboardViewModel
        {
            NombreLivres = livres.Count,
            NombreExemplaires = exemplaires.Count,
            ExemplairesDisponibles = exemplaires.Count(e => e.Statut == StatutExemplaire.Disponible),
            AdherentsActifs = adherents.Count(a => a.Actif),
            EmpruntsActifs = empruntsActifs.Count,
            EmpruntsEnRetard = empruntsActifs.Count(e => aujourdHui > e.DateEcheance.Date),
            PenalitesARegler = penalitesARegler.Count,
            MontantPenalitesARegler = penalitesARegler.Sum(p => p.Montant),
            DerniersEmprunts = emprunts
                .OrderByDescending(e => e.DateEmprunt)
                .ThenByDescending(e => e.Id)
                .Take(6)
                .Select(e =>
                {
                    exemplairesParId.TryGetValue(e.ExemplaireId, out var ex);
                    var titre = ex is not null && livresTitres.TryGetValue(ex.LivreId, out var t) ? t : "—";
                    return new DashboardEmpruntLigne
                    {
                        Id = e.Id,
                        AdherentNom = adherentsNoms.TryGetValue(e.AdherentId, out var nom) ? nom : "—",
                        LivreTitre = titre,
                        DateEmprunt = e.DateEmprunt,
                        DateEcheance = e.DateEcheance,
                        EstActif = e.DateRetour is null,
                        EnRetard = e.DateRetour is null && aujourdHui > e.DateEcheance.Date
                    };
                }).ToList()
        };

        return View(vm);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
