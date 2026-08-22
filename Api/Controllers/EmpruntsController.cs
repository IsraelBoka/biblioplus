using Api.Contracts;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/emprunts")]
public class EmpruntsController : ControllerBase
{
    private readonly ICirculationService _circulation;

    public EmpruntsController(ICirculationService circulation) => _circulation = circulation;

    [HttpPost]
    public async Task<ActionResult<EmpruntResponse>> Creer(CreerEmpruntRequest request, CancellationToken ct)
    {
        var result = await _circulation.EmprunterAsync(request.AdherentId, request.ExemplaireId, ct);
        if (!result.EstSucces) return this.ToErrorResponse(result);

        var v = result.Valeur!;
        var response = new EmpruntResponse(v.EmpruntId, v.DateEmprunt, v.DateEcheance);
        return CreatedAtAction(nameof(Creer), new { id = v.EmpruntId }, response);
    }

    [HttpPost("{id:int}/retour")]
    public async Task<ActionResult<RetourResponse>> Retour(int id, CancellationToken ct)
    {
        var result = await _circulation.RetournerAsync(id, ct);
        if (!result.EstSucces) return this.ToErrorResponse(result);

        var v = result.Valeur!;
        return Ok(new RetourResponse(v.EmpruntId, v.DateRetour, v.EnRetard, v.JoursRetard, v.MontantPenalite, v.PenaliteId));
    }
}
