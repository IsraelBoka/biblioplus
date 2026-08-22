using System.Linq.Expressions;
using Application.Abstractions;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

/// <summary>
/// Implémentation générique. Ne sauvegarde jamais : c'est l'Unit of Work qui persiste.
/// Le filtre global de suppression logique s'applique automatiquement aux requêtes.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly BiblioPlusContext Context;
    protected DbSet<T> Set => Context.Set<T>();

    public Repository(BiblioPlusContext context) => Context = context;

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(x => x.Id == id, ct);

    public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default)
        => await Set.ToListAsync(ct);

    public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => Set.AnyAsync(predicate, ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);
}
