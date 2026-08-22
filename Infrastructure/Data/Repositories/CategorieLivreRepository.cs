using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class CategorieLivreRepository : Repository<CategorieLivre>, ICategorieLivreRepository
{
    public CategorieLivreRepository(BiblioPlusContext context) : base(context) { }

    public Task<bool> CodeExisteAsync(string code, int? exclureId = null, CancellationToken ct = default)
        => Set.AnyAsync(x => x.Code == code && (exclureId == null || x.Id != exclureId), ct);

    public Task<bool> ADesLivresActifsAsync(int categorieId, CancellationToken ct = default)
        => Context.Livres.AnyAsync(l => l.CategorieLivreId == categorieId, ct);
}
