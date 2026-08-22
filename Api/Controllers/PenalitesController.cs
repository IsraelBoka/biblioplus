using Api.Contracts;
using Application.Abstractions;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/penalites")]
public class PenalitesController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public PenalitesController(IUnitOfWork uow) => _uow = uow;

    /// <summary>Règle une pénalité (passe son état à Reglee).</summary>
    [HttpPatch("{id:int}/regler")]
    public async Task<ActionResult<PenaliteResponse>> Regler(int id, CancellationToken ct)
    {
        var penalite = await _uow.Penalites.GetByIdAsync(id, ct);
        if (penalite is null) return NotFound();

        if (penalite.Etat != EtatPenalite.ARegler)
            return Conflict(new { erreur = $"La pénalité n'est pas à régler (état actuel : {penalite.Etat})." });

        penalite.Etat = EtatPenalite.Reglee;
        _uow.Penalites.Update(penalite);
        await _uow.SaveChangesAsync(ct);

        return Ok(new PenaliteResponse(
            penalite.Id, penalite.AdherentId, penalite.EmpruntId,
            penalite.Motif, penalite.Montant, penalite.Etat.ToString()));
    }
}
