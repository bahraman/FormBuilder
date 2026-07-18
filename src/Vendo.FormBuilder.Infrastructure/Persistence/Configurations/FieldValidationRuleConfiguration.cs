using Vendo.FormBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Configurations;

public sealed class FieldValidationRuleConfiguration : IEntityTypeConfiguration<FieldValidationRule>
{
    public void Configure(EntityTypeBuilder<FieldValidationRule> builder)
    {
        builder.ToTable("FieldValidationRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RuleType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new { x.FormFieldId, x.RuleType })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
