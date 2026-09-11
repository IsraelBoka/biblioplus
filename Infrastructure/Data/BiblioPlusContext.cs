using System.Linq.Expressions;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class BiblioPlusContext : DbContext
{
    public BiblioPlusContext(DbContextOptions<BiblioPlusContext> options) : base(options)
    {
    }

    public DbSet<CategorieLivre> CategoriesLivres => Set<CategorieLivre>();
    public DbSet<Livre> Livres => Set<Livre>();
    public DbSet<Exemplaire> Exemplaires => Set<Exemplaire>();
    public DbSet<Adherent> Adherents => Set<Adherent>();
    public DbSet<Emprunt> Emprunts => Set<Emprunt>();
    public DbSet<Penalite> Penalites => Set<Penalite>();
    public DbSet<ParametresCirculation> ParametresCirculation => Set<ParametresCirculation>();
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applique toutes les IEntityTypeConfiguration de cet assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BiblioPlusContext).Assembly);

        // Filtre global de suppression logique sur toutes les entités dérivant de BaseEntity.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyAccess = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(propertyAccess), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges();
    }

    private void ApplyAuditAndSoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;

                // La suppression est toujours logique : on intercepte le Delete.
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }
    }
}
