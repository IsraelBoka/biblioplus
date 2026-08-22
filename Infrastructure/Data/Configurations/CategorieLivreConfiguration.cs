using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CategorieLivreConfiguration : IEntityTypeConfiguration<CategorieLivre>
{
    public void Configure(EntityTypeBuilder<CategorieLivre> entity)
    {
        entity.Property(x => x.PenaliteParJour).HasColumnType("decimal(10,2)");

        // Index unique pertinent, compatible avec la suppression logique.
        entity.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        entity.HasMany(x => x.Livres)
            .WithOne(l => l.CategorieLivre)
            .HasForeignKey(l => l.CategorieLivreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
