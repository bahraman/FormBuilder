using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Enums;
using Vendo.FormBuilder.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Repositories;

public sealed class FormRepository : IFormRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FormRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Form?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Forms
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public Task<Form?> GetByIdWithDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Forms
            .Include(f => f.Fields)
                .ThenInclude(field => field.Options)
            .Include(f => f.Fields)
                .ThenInclude(field => field.ValidationRules)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Form> Items, int TotalCount)> GetPagedAsync(
        int subscriberId,
        int? restaurantId,
        int pageNumber,
        int pageSize,
        string? search = null,
        FormStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyTenantFilter(
                _dbContext.Forms.AsNoTracking().Include(f => f.Fields),
                subscriberId,
                restaurantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(f =>
                f.Name.Contains(term) ||
                f.Slug.Contains(term) ||
                (f.Description != null && f.Description.Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> SlugExistsAsync(
        int subscriberId,
        int? restaurantId,
        string slug,
        long? excludeFormId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Forms.AsNoTracking()
            .Where(f =>
                f.SubscriberId == subscriberId &&
                f.RestaurantId == restaurantId &&
                f.Slug == slug);

        if (excludeFormId.HasValue)
        {
            query = query.Where(f => f.Id != excludeFormId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetLatestVersionAsync(
        int subscriberId,
        int? restaurantId,
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Forms
            .AsNoTracking()
            .Where(f =>
                f.SubscriberId == subscriberId &&
                f.RestaurantId == restaurantId &&
                f.Slug == slug)
            .Select(f => (int?)f.Version)
            .MaxAsync(cancellationToken) ?? 0;
    }

    public async Task AddAsync(Form form, CancellationToken cancellationToken = default)
    {
        await _dbContext.Forms.AddAsync(form, cancellationToken);
    }

    public void Update(Form form)
    {
        var entry = _dbContext.Entry(form);
        if (entry.State == EntityState.Detached)
        {
            // Only attach detached roots. Never call Update() on a tracked aggregate graph:
            // EF would mark newly added children as Modified with empty RowVersion tokens.
            _dbContext.Forms.Attach(form);
            entry.State = EntityState.Modified;
        }
    }

    public void SetOriginalRowVersion(Form form, byte[] rowVersion)
    {
        _dbContext.Entry(form).Property(x => x.RowVersion).OriginalValue = rowVersion;
    }

    public void SetOriginalRowVersion(FormField field, byte[] rowVersion)
    {
        _dbContext.Entry(field).Property(x => x.RowVersion).OriginalValue = rowVersion;
    }

    public void AddField(FormField field)
    {
        _dbContext.FormFields.Add(field);
    }

    /// <summary>
    /// Subscriber is always required.
    /// When restaurantId is provided: restaurant-specific forms for that restaurant + subscriber-level (null) forms.
    /// When restaurantId is null: all forms for the subscriber.
    /// </summary>
    private static IQueryable<Form> ApplyTenantFilter(
        IQueryable<Form> query,
        int subscriberId,
        int? restaurantId)
    {
        query = query.Where(f => f.SubscriberId == subscriberId);

        if (restaurantId.HasValue)
        {
            query = query.Where(f => f.RestaurantId == null || f.RestaurantId == restaurantId);
        }

        return query;
    }
}
