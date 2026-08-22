using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Contracts;

/// <summary>Traduit un échec métier <see cref="Result"/> en réponse HTTP appropriée.</summary>
public static class ResultMapping
{
    public static ActionResult ToErrorResponse(this ControllerBase controller, Result result)
    {
        var payload = new { erreur = result.Erreur };
        return result.TypeErreur switch
        {
            ErreurType.Introuvable => controller.NotFound(payload),   // 404
            ErreurType.Conflit => controller.Conflict(payload),       // 409
            ErreurType.Validation => controller.BadRequest(payload),  // 400
            _ => controller.BadRequest(payload)
        };
    }
}
