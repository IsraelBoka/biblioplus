using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class LivreRepository : Repository<Livre>, ILivreRepository
{
    public LivreRepository(BiblioPlusContext context) : base(context) { }

    public async Task<IReadOnlyList<Livre>> GetDisponiblesAsync(string? recherche, CancellationToken ct = default)
    {
        var query = Set.Include(l => l.CategorieLivre).AsQueryable();

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = $"%{recherche.Trim()}%";
            query = query.Where(l =>
                EF.Functions.Like(l.Titre, terme) ||
                EF.Functions.Like(l.Auteur, terme) ||
                EF.Functions.Like(l.Isbn, terme));
        }

        // Disponibilité : au moins un exemplaire disponible (le filtre global exclut les supprimés).
        query = query.Where(l => l.Exemplaires.Any(e => e.Statut == StatutExemplaire.Disponible));

        return await query.OrderBy(l => l.Titre).ToListAsync(ct);
    }
}
