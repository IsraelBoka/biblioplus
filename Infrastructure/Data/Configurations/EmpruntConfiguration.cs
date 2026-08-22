using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class EmpruntConfiguration : IEntityTypeConfiguration<Emprunt>
{
    public void Configure(EntityTypeBuilder<Emprunt> entity)
    {
        entity.Ignore(x => x.EstActif);

        // Aide aux requêtes de disponibilité / emprunts actifs.
        entity.HasIndex(x => new { x.ExemplaireId, x.DateRetour });
        entity.HasIndex(x => new { x.AdherentId, x.DateRetour });

        // Relation 1—0..1 avec la pénalité.
        entity.HasOne(x => x.Penalite)
            .WithOne(p => p.Emprunt)
            .HasForeignKey<Penalite>(p => p.EmpruntId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
