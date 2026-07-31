using MediatR;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Locations.Dtos;
using Vendo.FormBuilder.Domain.Interfaces;

namespace Vendo.FormBuilder.Application.Locations.Queries.GetProvinces;

public sealed record GetProvincesQuery : IRequest<IReadOnlyList<ProvinceDto>>;

public sealed class GetProvincesQueryHandler
    : IRequestHandler<GetProvincesQuery, IReadOnlyList<ProvinceDto>>
{
    private readonly ILocationRepository _locationRepository;

    public GetProvincesQueryHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<IReadOnlyList<ProvinceDto>> Handle(
        GetProvincesQuery request,
        CancellationToken cancellationToken)
    {
        var provinces = await _locationRepository.GetProvincesAsync(cancellationToken);
        return provinces.Select(p => p.ToDto()).ToList();
    }
}
