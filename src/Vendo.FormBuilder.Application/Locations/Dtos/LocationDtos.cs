namespace Vendo.FormBuilder.Application.Locations.Dtos;

public sealed class ProvinceDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int OrderIndex { get; init; }
}

public sealed class CityDto
{
    public required int Id { get; init; }
    public required int ProvinceId { get; init; }
    public required string Name { get; init; }
    public required int OrderIndex { get; init; }
}
