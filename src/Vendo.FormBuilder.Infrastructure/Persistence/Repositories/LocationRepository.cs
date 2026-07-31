using Microsoft.EntityFrameworkCore;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Interfaces;

namespace Vendo.FormBuilder.Infrastructure.Persistence.Repositories;

public sealed class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LocationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Province>> GetProvincesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Provinces
            .AsNoTracking()
            .OrderBy(p => p.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ProvinceExistsAsync(int provinceId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Provinces
            .AsNoTracking()
            .AnyAsync(p => p.Id == provinceId, cancellationToken);
    }

    public async Task<IReadOnlyList<City>> GetCitiesByProvinceAsync(
        int provinceId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cities
            .AsNoTracking()
            .Where(c => c.ProvinceId == provinceId)
            .OrderBy(c => c.OrderIndex)
            .ToListAsync(cancellationToken);
    }
}
