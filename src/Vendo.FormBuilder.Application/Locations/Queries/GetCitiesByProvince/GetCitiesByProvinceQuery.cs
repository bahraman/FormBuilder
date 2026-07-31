using MediatR;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Locations.Dtos;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;

namespace Vendo.FormBuilder.Application.Locations.Queries.GetCitiesByProvince;

public sealed record GetCitiesByProvinceQuery(int ProvinceId) : IRequest<IReadOnlyList<CityDto>>;

public sealed class GetCitiesByProvinceQueryHandler
    : IRequestHandler<GetCitiesByProvinceQuery, IReadOnlyList<CityDto>>
{
    private readonly ILocationRepository _locationRepository;

    public GetCitiesByProvinceQueryHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<IReadOnlyList<CityDto>> Handle(
        GetCitiesByProvinceQuery request,
        CancellationToken cancellationToken)
    {
        var provinceExists = await _locationRepository.ProvinceExistsAsync(request.ProvinceId, cancellationToken);
        if (!provinceExists)
        {
            throw new NotFoundException(nameof(Province), request.ProvinceId);
        }

        var cities = await _locationRepository.GetCitiesByProvinceAsync(request.ProvinceId, cancellationToken);
        return cities.Select(c => c.ToDto()).ToList();
    }
}
