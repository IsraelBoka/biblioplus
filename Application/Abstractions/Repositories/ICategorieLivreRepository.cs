using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ICategorieLivreRepository : IRepository<CategorieLivre>
{
    /// <summary>Vrai si une autre catégorie active utilise déjà ce code (unicité).</summary>
    Task<bool> CodeExisteAsync(string code, int? exclureId = null, CancellationToken ct = default);

    /// <summary>Vrai si la catégorie possède encore des livres actifs (refus de suppression).</summary>
    Task<bool> ADesLivresActifsAsync(int categorieId, CancellationToken ct = default);
}
