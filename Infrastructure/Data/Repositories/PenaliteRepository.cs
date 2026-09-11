using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class PenaliteRepository : Repository<Penalite>, IPenaliteRepository
{
    public PenaliteRepository(BiblioPlusContext context) : base(context) { }

    public Task<bool> AdherentADesPenalitesNonRegleesAsync(int adherentId, CancellationToken ct = default)
        => Set.AnyAsync(p => p.AdherentId == adherentId && p.Etat == EtatPenalite.ARegler, ct);

    public async Task<IReadOnlyList<Penalite>> ListParAdherentAsync(int adherentId, CancellationToken ct = default)
        // SQLite ne sait pas trier sur DateTimeOffset : l'Id (auto-incrément) suit l'ordre de création.
        => await Set.Where(p => p.AdherentId == adherentId)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync(ct);
}
