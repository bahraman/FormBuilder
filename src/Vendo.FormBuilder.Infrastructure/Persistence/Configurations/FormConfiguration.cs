using Vendo.FormBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Configurations;

public sealed class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("Forms");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(x => x.SubscriberId)
            .IsRequired();

        builder.Property(x => x.RestaurantId);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Slug uniqueness is scoped per tenant ownership (subscriber + restaurant).
        builder.HasIndex(x => new { x.SubscriberId, x.RestaurantId, x.Slug, x.Version })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.SubscriberId);
        builder.HasIndex(x => new { x.SubscriberId, x.RestaurantId });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Name);

        builder.HasOne(x => x.ParentForm)
            .WithMany()
            .HasForeignKey(x => x.ParentFormId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Fields)
            .WithOne(x => x.Form)
            .HasForeignKey(x => x.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Responses)
            .WithOne(x => x.Form)
            .HasForeignKey(x => x.FormId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(Form.Fields))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(Form.Responses))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
