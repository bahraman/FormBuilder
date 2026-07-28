using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Infrastructure.Persistence.Seed;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Configurations;

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(x => x.Id);

        // Reference data ships with fixed ids so seeded rows stay stable.
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ProvinceId, x.OrderIndex });
        builder.HasIndex(x => new { x.ProvinceId, x.Name }).IsUnique();

        builder.HasData(IranLocationSeedData.Cities);
    }
}
