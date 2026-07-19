using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc ??= DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Fixes false optimistic-concurrency failures:
    /// new aggregate children incorrectly marked Modified (empty original RowVersion),
    /// and tracked parents whose OriginalValue was cleared.
    /// </summary>
    private void NormalizeRowVersionEntityStates()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().ToList())
        {
            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            var rowVersionProperty = entry.Property(nameof(BaseEntity.RowVersion));
            var original = rowVersionProperty.OriginalValue as byte[];
            var current = rowVersionProperty.CurrentValue as byte[];

            var originalMissing = original is null || original.Length == 0;
            var currentMissing = current is null || current.Length == 0;

            if (originalMissing && currentMissing)
            {
                // Never persisted. Soft-deleted drafts should not be written; others are inserts.
                entry.State = entry.Entity.IsDeleted ? EntityState.Detached : EntityState.Added;
                continue;
            }

            if (originalMissing && !currentMissing)
            {
                // Keep concurrency check against the known token loaded on the entity.
                rowVersionProperty.OriginalValue = current;
            }
        }
    }

    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
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
        where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
