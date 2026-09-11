using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

/// <summary>
/// Accès à la ligne unique des paramètres de circulation. Ne sauvegarde jamais :
/// la persistance est déclenchée par l'Unit of Work.
/// </summary>
public class ParametresRepository : IParametresRepository
{
    private readonly BiblioPlusContext _context;

    public ParametresRepository(BiblioPlusContext context) => _context = context;

    public async Task<ParametresCirculation> GetAsync(CancellationToken ct = default)
    {
        var parametres = await _context.Set<ParametresCirculation>().FirstOrDefaultAsync(ct);
        if (parametres is null)
        {
            // Aucune configuration : on crée une instance par défaut (suivie, persistée au prochain SaveChanges).
            parametres = new ParametresCirculation();
            await _context.Set<ParametresCirculation>().AddAsync(parametres, ct);
        }
        return parametres;
    }

    public void Update(ParametresCirculation parametres)
        => _context.Set<ParametresCirculation>().Update(parametres);
}
