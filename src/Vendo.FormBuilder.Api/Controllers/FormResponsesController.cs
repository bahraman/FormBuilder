using Vendo.FormBuilder.Application.Common.Models;
using Vendo.FormBuilder.Application.Responses.Commands.SubmitFormResponse;
using Vendo.FormBuilder.Application.Responses.Dtos;
using Vendo.FormBuilder.Application.Responses.Queries.GetFormResponseById;
using Vendo.FormBuilder.Application.Responses.Queries.GetFormResponses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Vendo.FormBuilder.Api.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class FormResponsesController : ControllerBase
{
    private readonly ISender _sender;

    public FormResponsesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Submit a response to a published form within the caller's tenant scope.
    /// </summary>
    [HttpPost("forms/{formId:long}/responses")]
    [ProducesResponseType(typeof(FormResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormResponseDto>> SubmitResponse(
        long formId,
        [FromQuery] int subscriberId,
        [FromBody] SubmitFormResponseRequest request,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new SubmitFormResponseCommand(
                formId,
                subscriberId,
                restaurantId,
                request.Values,
                request.SubmittedBy,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetResponseById),
            new { responseId = result.Id, subscriberId, restaurantId },
            result);
    }

    /// <summary>
    /// Get paginated responses for a form within the caller's tenant scope.
    /// </summary>
    [HttpGet("forms/{formId:long}/responses")]
    [ProducesResponseType(typeof(PagedResult<FormResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<FormResponseDto>>> GetResponses(
        long formId,
        [FromQuery] int subscriberId,
        [FromQuery] int? restaurantId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFormResponsesQuery(formId, subscriberId, restaurantId, pageNumber, pageSize),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get a single form response by id within the caller's tenant scope.
    /// </summary>
    [HttpGet("responses/{responseId:guid}")]
    [ProducesResponseType(typeof(FormResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormResponseDto>> GetResponseById(
        Guid responseId,
        [FromQuery] int subscriberId,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFormResponseByIdQuery(responseId, subscriberId, restaurantId),
            cancellationToken);
        return Ok(result);
    }
}

public sealed record SubmitFormResponseRequest(
    IReadOnlyList<FormResponseValueInputDto> Values,
    string? SubmittedBy = null);
