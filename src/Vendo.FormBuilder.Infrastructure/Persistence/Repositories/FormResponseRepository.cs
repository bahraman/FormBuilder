using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Repositories;

public sealed class FormResponseRepository : IFormResponseRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FormResponseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FormResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.FormResponses
            .AsNoTracking()
            .Include(r => r.Values)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<FormResponse> Items, int TotalCount)> GetByFormIdPagedAsync(
        Guid formId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FormResponses
            .AsNoTracking()
            .Include(r => r.Values)
            .Where(r => r.FormId == formId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.SubmittedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(FormResponse response, CancellationToken cancellationToken = default)
    {
        await _dbContext.FormResponses.AddAsync(response, cancellationToken);
    }
}
