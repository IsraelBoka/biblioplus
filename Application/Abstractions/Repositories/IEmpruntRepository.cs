using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IEmpruntRepository : IRepository<Emprunt>
{
    /// <summary>Nombre d'emprunts actifs (sans date de retour) d'un adhérent.</summary>
    Task<int> CompterActifsParAdherentAsync(int adherentId, CancellationToken ct = default);

    /// <summary>Charge un emprunt actif avec son exemplaire, son livre et la catégorie (pour le retour).</summary>
    Task<Emprunt?> GetActifAvecCategorieAsync(int empruntId, CancellationToken ct = default);
}
