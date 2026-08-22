using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class AdherentConfiguration : IEntityTypeConfiguration<Adherent>
{
    public void Configure(EntityTypeBuilder<Adherent> entity)
    {
        entity.HasIndex(x => x.Numero)
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        entity.HasMany(x => x.Emprunts)
            .WithOne(e => e.Adherent)
            .HasForeignKey(e => e.AdherentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.Penalites)
            .WithOne(p => p.Adherent)
            .HasForeignKey(p => p.AdherentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
