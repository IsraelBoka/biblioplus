using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IExemplaireRepository : IRepository<Exemplaire>
{
    /// <summary>Charge l'exemplaire avec son livre et la catégorie (nécessaire au calcul d'échéance).</summary>
    Task<Exemplaire?> GetAvecLivreEtCategorieAsync(int exemplaireId, CancellationToken ct = default);
}
