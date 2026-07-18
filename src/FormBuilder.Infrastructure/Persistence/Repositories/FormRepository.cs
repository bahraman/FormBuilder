using FormBuilder.Domain.Entities;
using FormBuilder.Domain.Enums;
using FormBuilder.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Persistence.Repositories;

public sealed class FormRepository : IFormRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FormRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Form?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Forms
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public Task<Form?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Forms
            .Include(f => f.Fields)
                .ThenInclude(field => field.Options)
            .Include(f => f.Fields)
                .ThenInclude(field => field.ValidationRules)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Form> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        FormStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Forms
            .AsNoTracking()
            .Include(f => f.Fields)
            .AsQueryable();

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
        string slug,
        Guid? excludeFormId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Forms.AsNoTracking().Where(f => f.Slug == slug);

        if (excludeFormId.HasValue)
        {
            query = query.Where(f => f.Id != excludeFormId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetLatestVersionAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Forms
            .AsNoTracking()
            .Where(f => f.Slug == slug)
            .Select(f => (int?)f.Version)
            .MaxAsync(cancellationToken) ?? 0;
    }

    public async Task AddAsync(Form form, CancellationToken cancellationToken = default)
    {
        await _dbContext.Forms.AddAsync(form, cancellationToken);
    }

    public void Update(Form form)
    {
        _dbContext.Forms.Update(form);
    }
}
