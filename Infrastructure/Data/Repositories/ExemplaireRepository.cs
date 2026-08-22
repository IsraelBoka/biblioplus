using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class ExemplaireRepository : Repository<Exemplaire>, IExemplaireRepository
{
    public ExemplaireRepository(BiblioPlusContext context) : base(context) { }

    public Task<Exemplaire?> GetAvecLivreEtCategorieAsync(int exemplaireId, CancellationToken ct = default)
        => Set.Include(e => e.Livre!)
              .ThenInclude(l => l.CategorieLivre)
              .FirstOrDefaultAsync(e => e.Id == exemplaireId, ct);
}
