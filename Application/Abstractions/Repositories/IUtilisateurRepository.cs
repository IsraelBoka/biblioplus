using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IUtilisateurRepository : IRepository<Utilisateur>
{
    Task<Utilisateur?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExisteAsync(string email, int? exclureId = null, CancellationToken ct = default);
}
