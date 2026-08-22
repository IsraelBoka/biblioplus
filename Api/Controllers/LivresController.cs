using Api.Contracts;
using Application.Abstractions;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/livres")]
public class LivresController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public LivresController(IUnitOfWork uow) => _uow = uow;

    /// <summary>Livres possédant au moins un exemplaire disponible, filtrés par recherche.</summary>
    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<LivreDisponibleResponse>>> GetDisponibles(
        [FromQuery] string? recherche, CancellationToken ct)
    {
        var livres = await _uow.Livres.GetDisponiblesAsync(recherche, ct);

        var response = livres.Select(l => new LivreDisponibleResponse(
            l.Id,
            l.Isbn,
            l.Titre,
            l.Auteur,
            l.CategorieLivre?.Libelle ?? string.Empty,
            l.Exemplaires.Count(e => e.Statut == StatutExemplaire.Disponible)));

        return Ok(response);
    }
}
