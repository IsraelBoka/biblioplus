using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ExemplaireConfiguration : IEntityTypeConfiguration<Exemplaire>
{
    public void Configure(EntityTypeBuilder<Exemplaire> entity)
    {
        entity.Property(x => x.Statut).HasConversion<int>();

        entity.HasIndex(x => x.CodeBarres)
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        entity.HasMany(x => x.Emprunts)
            .WithOne(e => e.Exemplaire)
            .HasForeignKey(e => e.ExemplaireId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
