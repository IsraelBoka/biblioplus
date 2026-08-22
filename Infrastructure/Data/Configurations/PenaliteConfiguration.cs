using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PenaliteConfiguration : IEntityTypeConfiguration<Penalite>
{
    public void Configure(EntityTypeBuilder<Penalite> entity)
    {
        entity.Property(x => x.Montant).HasColumnType("decimal(10,2)");
        entity.Property(x => x.Etat).HasConversion<int>();

        // Une seule pénalité par emprunt (unicité de la relation 1—0..1).
        entity.HasIndex(x => x.EmpruntId)
            .IsUnique()
            .HasFilter("IsDeleted = 0");
    }
}
