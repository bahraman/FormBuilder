using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Vendo.FormBuilder.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<FieldOption> FieldOptions => Set<FieldOption>();
    public DbSet<FieldValidationRule> FieldValidationRules => Set<FieldValidationRule>();
    public DbSet<FormResponse> FormResponses => Set<FormResponse>();
    public DbSet<FormResponseValue> FormResponseValues => Set<FormResponseValue>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<City> Cities => Set<City>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplySoftDeleteFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        PrepareForSave();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PrepareForSave();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void PrepareForSave()
    {
        ChangeTracker.DetectChanges();
        NormalizeRowVersionEntityStates();
        MarkTemporaryLongIdentityKeys();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc ??= DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// LongEntity assigns negative in-memory Ids so graphs stay unique before INSERT.
    /// SQL Server IDENTITY rejects explicit Ids unless IDENTITY_INSERT is ON, so tell EF
    /// those values are temporary; store-generated values replace them (and matching FKs).
    /// </summary>
    private void MarkTemporaryLongIdentityKeys()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added || entry.Entity is not LongEntity)
            {
                continue;
            }

            var idProperty = entry.Property(nameof(LongEntity.Id));
            if (idProperty.Metadata.ValueGenerated == ValueGenerated.OnAdd
                && idProperty.CurrentValue is long id
                && id < 0)
            {
                idProperty.IsTemporary = true;
            }
        }
    }

    private void NormalizeRowVersionEntityStates()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>().ToList())
        {
            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            var rowVersionProperty = entry.Property(nameof(AuditableEntity.RowVersion));
            var original = rowVersionProperty.OriginalValue as byte[];
            var current = rowVersionProperty.CurrentValue as byte[];

            var originalMissing = original is null || original.Length == 0;
            var currentMissing = current is null || current.Length == 0;

            if (originalMissing && currentMissing)
            {
                entry.State = entry.Entity.IsDeleted ? EntityState.Detached : EntityState.Added;
                continue;
            }

            if (originalMissing && !currentMissing)
            {
                rowVersionProperty.OriginalValue = current;
            }
        }
    }

    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var method = typeof(ApplicationDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, [modelBuilder]);
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : AuditableEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
