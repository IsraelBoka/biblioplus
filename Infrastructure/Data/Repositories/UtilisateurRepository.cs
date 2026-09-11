using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class UtilisateurRepository : Repository<Utilisateur>, IUtilisateurRepository
{
    public UtilisateurRepository(BiblioPlusContext context) : base(context) { }

    public Task<Utilisateur?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Set.Include(u => u.Adherent)
              .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExisteAsync(string email, int? exclureId = null, CancellationToken ct = default)
        => Set.AnyAsync(u => u.Email == email && (exclureId == null || u.Id != exclureId), ct);
}
