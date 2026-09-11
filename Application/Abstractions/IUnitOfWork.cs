using Application.Abstractions.Repositories;

namespace Application.Abstractions;

/// <summary>
/// Regroupe les repositories autour d'un seul DbContext et centralise la sauvegarde.
/// Garantit qu'un cas d'usage réussi appelle <see cref="SaveChangesAsync"/> une seule fois.
/// </summary>
public interface IUnitOfWork
{
    ICategorieLivreRepository CategoriesLivres { get; }
    ILivreRepository Livres { get; }
    IExemplaireRepository Exemplaires { get; }
    IAdherentRepository Adherents { get; }
    IEmpruntRepository Emprunts { get; }
    IPenaliteRepository Penalites { get; }
    IParametresRepository Parametres { get; }
    IUtilisateurRepository Utilisateurs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
