using System.Linq.Expressions;
using Domain.Common;

namespace Application.Abstractions;

/// <summary>
/// Repository générique en lecture/écriture. Les méthodes NE sauvegardent PAS :
/// la persistance est déclenchée une seule fois via <see cref="IUnitOfWork.SaveChangesAsync"/>.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);

    /// <summary>Marque l'entité pour suppression (convertie en suppression logique au SaveChanges).</summary>
    void Remove(T entity);
}
