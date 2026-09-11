using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ParametresCirculationConfiguration : IEntityTypeConfiguration<ParametresCirculation>
{
    public void Configure(EntityTypeBuilder<ParametresCirculation> entity)
    {
        entity.Property(x => x.PlafondPenalite).HasColumnType("decimal(10,2)");
    }
}
