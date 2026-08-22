using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class AdherentRepository : Repository<Adherent>, IAdherentRepository
{
    public AdherentRepository(BiblioPlusContext context) : base(context) { }

    public Task<bool> NumeroExisteAsync(string numero, int? exclureId = null, CancellationToken ct = default)
        => Set.AnyAsync(a => a.Numero == numero && (exclureId == null || a.Id != exclureId), ct);
}
