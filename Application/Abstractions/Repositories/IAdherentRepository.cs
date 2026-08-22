using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IAdherentRepository : IRepository<Adherent>
{
    Task<bool> NumeroExisteAsync(string numero, int? exclureId = null, CancellationToken ct = default);
}
