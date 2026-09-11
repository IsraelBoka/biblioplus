using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class EmpruntRepository : Repository<Emprunt>, IEmpruntRepository
{
    public EmpruntRepository(BiblioPlusContext context) : base(context) { }

    public Task<int> CompterActifsParAdherentAsync(int adherentId, CancellationToken ct = default)
        => Set.CountAsync(e => e.AdherentId == adherentId && e.DateRetour == null, ct);

    public Task<Emprunt?> GetActifAvecCategorieAsync(int empruntId, CancellationToken ct = default)
        => Set.Include(e => e.Exemplaire!)
              .ThenInclude(x => x.Livre!)
              .ThenInclude(l => l.CategorieLivre)
              .FirstOrDefaultAsync(e => e.Id == empruntId && e.DateRetour == null, ct);

    public async Task<IReadOnlyList<Emprunt>> ListParAdherentAsync(int adherentId, CancellationToken ct = default)
        => await Set.Include(e => e.Exemplaire!)
                    .ThenInclude(x => x.Livre)
                    .Where(e => e.AdherentId == adherentId)
                    .OrderByDescending(e => e.DateEmprunt)
                    .ThenByDescending(e => e.Id)
                    .ToListAsync(ct);
}
