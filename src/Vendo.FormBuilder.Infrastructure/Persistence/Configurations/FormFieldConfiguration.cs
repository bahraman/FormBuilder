using Vendo.FormBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Configurations;

public sealed class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.ToTable("FormFields");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FieldType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Placeholder).HasMaxLength(500);
        builder.Property(x => x.HelpText).HasMaxLength(1000);
        builder.Property(x => x.DefaultValue).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new { x.FormId, x.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => new { x.FormId, x.DisplayOrder });

        builder.HasMany(x => x.Options)
            .WithOne(x => x.FormField)
            .HasForeignKey(x => x.FormFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ValidationRules)
            .WithOne(x => x.FormField)
            .HasForeignKey(x => x.FormFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(FormField.Options))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(FormField.ValidationRules))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
