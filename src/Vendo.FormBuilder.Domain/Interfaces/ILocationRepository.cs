using Vendo.FormBuilder.Domain.Entities;

namespace Vendo.FormBuilder.Domain.Interfaces;

/// <summary>
/// Read-only access to the province and city lookups. These are reference data,
/// so no write operations are exposed.
/// </summary>
public interface ILocationRepository
{
    Task<IReadOnlyList<Province>> GetProvincesAsync(CancellationToken cancellationToken = default);

    Task<bool> ProvinceExistsAsync(int provinceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<City>> GetCitiesByProvinceAsync(int provinceId, CancellationToken cancellationToken = default);
}
