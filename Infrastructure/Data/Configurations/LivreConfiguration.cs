using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class LivreConfiguration : IEntityTypeConfiguration<Livre>
{
    public void Configure(EntityTypeBuilder<Livre> entity)
    {
        entity.HasIndex(x => x.Isbn)
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        entity.HasMany(x => x.Exemplaires)
            .WithOne(e => e.Livre)
            .HasForeignKey(e => e.LivreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
