using Vendo.FormBuilder.Application.Locations.Dtos;
using Vendo.FormBuilder.Domain.Entities;

namespace Vendo.FormBuilder.Application.Common.Mappings;

public static class LocationMappings
{
    public static ProvinceDto ToDto(this Province province) => new()
    {
        Id = province.Id,
        Name = province.Name,
        OrderIndex = province.OrderIndex
    };

    public static CityDto ToDto(this City city) => new()
    {
        Id = city.Id,
        ProvinceId = city.ProvinceId,
        Name = city.Name,
        OrderIndex = city.OrderIndex
    };
}
