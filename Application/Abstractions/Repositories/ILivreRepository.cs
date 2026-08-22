using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ILivreRepository : IRepository<Livre>
{
    /// <summary>
    /// Livres possédant au moins un exemplaire disponible, filtrés par titre / auteur / ISBN.
    /// Requête spécialisée (Where + Any + disponibilité).
    /// </summary>
    Task<IReadOnlyList<Livre>> GetDisponiblesAsync(string? recherche, CancellationToken ct = default);
}
