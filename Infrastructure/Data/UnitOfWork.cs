using Application.Abstractions;
using Application.Abstractions.Repositories;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data;

/// <summary>
/// Partage un seul <see cref="BiblioPlusContext"/> entre tous les repositories
/// et centralise la sauvegarde.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly BiblioPlusContext _context;

    public UnitOfWork(BiblioPlusContext context)
    {
        _context = context;
        CategoriesLivres = new CategorieLivreRepository(context);
        Livres = new LivreRepository(context);
        Exemplaires = new ExemplaireRepository(context);
        Adherents = new AdherentRepository(context);
        Emprunts = new EmpruntRepository(context);
        Penalites = new PenaliteRepository(context);
        Parametres = new ParametresRepository(context);
        Utilisateurs = new UtilisateurRepository(context);
    }

    public ICategorieLivreRepository CategoriesLivres { get; }
    public ILivreRepository Livres { get; }
    public IExemplaireRepository Exemplaires { get; }
    public IAdherentRepository Adherents { get; }
    public IEmpruntRepository Emprunts { get; }
    public IPenaliteRepository Penalites { get; }
    public IParametresRepository Parametres { get; }
    public IUtilisateurRepository Utilisateurs { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
