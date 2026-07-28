using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vendo.FormBuilder.Application.Locations.Dtos;
using Vendo.FormBuilder.Application.Locations.Queries.GetCitiesByProvince;
using Vendo.FormBuilder.Application.Locations.Queries.GetProvinces;

namespace Vendo.FormBuilder.Api.Controllers;

/// <summary>
/// Read-only lookups backing the <c>Province</c> and <c>City</c> field types.
/// The data is reference data shared by every tenant, so no tenant scope is required.
/// </summary>
[ApiController]
[Route("api/provinces")]
[Produces("application/json")]
public sealed class ProvincesController : ControllerBase
{
    private readonly ISender _sender;

    public ProvincesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all provinces ordered by OrderIndex.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProvinceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProvinceDto>>> GetProvinces(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProvincesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get the cities of a province ordered by OrderIndex.
    /// </summary>
    [HttpGet("{provinceId:int}/cities")]
    [ProducesResponseType(typeof(IReadOnlyList<CityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCities(
        int provinceId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCitiesByProvinceQuery(provinceId), cancellationToken);
        return Ok(result);
    }
}
