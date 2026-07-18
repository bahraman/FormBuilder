using Vendo.FormBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Configurations;

public sealed class FormResponseValueConfiguration : IEntityTypeConfiguration<FormResponseValue>
{
    public void Configure(EntityTypeBuilder<FormResponseValue> builder)
    {
        builder.ToTable("FormResponseValues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FieldName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.FormResponseId);
        builder.HasIndex(x => x.FormFieldId);

        builder.HasOne(x => x.FormField)
            .WithMany()
            .HasForeignKey(x => x.FormFieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
