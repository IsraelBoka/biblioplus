using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>
/// Accès à la ligne unique des paramètres de circulation. Ne sauvegarde pas :
/// la persistance passe par <see cref="IUnitOfWork.SaveChangesAsync"/>.
/// </summary>
public interface IParametresRepository
{
    /// <summary>
    /// Retourne les paramètres courants. Si aucune ligne n'existe encore, une
    /// instance par défaut est créée (et suivie) pour éviter une absence de config.
    /// </summary>
    Task<ParametresCirculation> GetAsync(CancellationToken ct = default);

    void Update(ParametresCirculation parametres);
}
