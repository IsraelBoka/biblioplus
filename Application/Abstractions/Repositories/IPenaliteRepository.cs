using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IPenaliteRepository : IRepository<Penalite>
{
    /// <summary>Vrai si l'adhérent possède au moins une pénalité encore à régler.</summary>
    Task<bool> AdherentADesPenalitesNonRegleesAsync(int adherentId, CancellationToken ct = default);

    /// <summary>Pénalités d'un adhérent, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<Penalite>> ListParAdherentAsync(int adherentId, CancellationToken ct = default);
}
