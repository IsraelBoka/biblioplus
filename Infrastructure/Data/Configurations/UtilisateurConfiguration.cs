using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UtilisateurConfiguration : IEntityTypeConfiguration<Utilisateur>
{
    public void Configure(EntityTypeBuilder<Utilisateur> entity)
    {
        entity.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        entity.HasOne(x => x.Adherent)
            .WithMany()
            .HasForeignKey(x => x.AdherentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
