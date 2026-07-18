using FormBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormBuilder.Infrastructure.Persistence.Configurations;

public sealed class FormResponseConfiguration : IEntityTypeConfiguration<FormResponse>
{
    public void Configure(EntityTypeBuilder<FormResponse> builder)
    {
        builder.ToTable("FormResponses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubmittedBy).HasMaxLength(200);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.FormId);
        builder.HasIndex(x => x.SubmittedAtUtc);

        builder.HasMany(x => x.Values)
            .WithOne(x => x.FormResponse)
            .HasForeignKey(x => x.FormResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(FormResponse.Values))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
