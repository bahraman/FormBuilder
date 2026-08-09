using Vendo.FormBuilder.Application.Common.Models;
using Vendo.FormBuilder.Application.Responses.Commands.DeleteFormResponse;
using Vendo.FormBuilder.Application.Responses.Commands.SubmitFormResponse;
using Vendo.FormBuilder.Application.Responses.Commands.UpdateFormResponse;
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
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new SubmitFormResponseCommand(
                formId,
                subscriberId,
                request.Values,
                request.SubmittedBy,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetResponseById),
            new { responseId = result.Id, subscriberId },
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
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFormResponsesQuery(formId, subscriberId, pageNumber, pageSize),
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
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFormResponseByIdQuery(responseId, subscriberId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update an existing form response within the caller's tenant scope.
    /// </summary>
    [HttpPut("responses/{responseId:guid}")]
    [ProducesResponseType(typeof(FormResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormResponseDto>> UpdateResponse(
        Guid responseId,
        [FromQuery] int subscriberId,
        [FromBody] UpdateFormResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new UpdateFormResponseCommand(
                responseId,
                subscriberId,
                request.Values,
                request.UpdatedBy),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Soft-delete a form response within the caller's tenant scope.
    /// </summary>
    [HttpDelete("responses/{responseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteResponse(
        Guid responseId,
        [FromQuery] int subscriberId,
        [FromQuery] string? deletedBy = null,
        CancellationToken cancellationToken = default)
    {
        await _sender.Send(
            new DeleteFormResponseCommand(responseId, subscriberId, deletedBy),
            cancellationToken);

        return NoContent();
    }
}

public sealed record SubmitFormResponseRequest(
    IReadOnlyList<FormResponseValueInputDto> Values,
    string? SubmittedBy = null);

public sealed record UpdateFormResponseRequest(
    IReadOnlyList<FormResponseValueInputDto> Values,
    string? UpdatedBy = null);
