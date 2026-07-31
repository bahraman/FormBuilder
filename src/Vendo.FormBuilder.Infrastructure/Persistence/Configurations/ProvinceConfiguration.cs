using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Infrastructure.Persistence.Seed;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Configurations;

public sealed class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("Provinces");

        builder.HasKey(x => x.Id);

        // Reference data ships with fixed ids so seeded rows stay stable.
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.HasIndex(x => x.OrderIndex);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasData(IranLocationSeedData.Provinces);
    }
}
