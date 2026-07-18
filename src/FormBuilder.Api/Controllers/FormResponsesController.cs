using FormBuilder.Application.Common.Models;
using FormBuilder.Application.Responses.Commands.SubmitFormResponse;
using FormBuilder.Application.Responses.Dtos;
using FormBuilder.Application.Responses.Queries.GetFormResponseById;
using FormBuilder.Application.Responses.Queries.GetFormResponses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Api.Controllers;

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
    /// Submit a response to a published form.
    /// </summary>
    [HttpPost("forms/{formId:guid}/responses")]
    [ProducesResponseType(typeof(FormResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormResponseDto>> SubmitResponse(
        Guid formId,
        [FromBody] SubmitFormResponseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SubmitFormResponseCommand(
                formId,
                request.Values,
                request.SubmittedBy,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()),
            cancellationToken);

        return CreatedAtAction(nameof(GetResponseById), new { responseId = result.Id }, result);
    }

    /// <summary>
    /// Get paginated responses for a form.
    /// </summary>
    [HttpGet("forms/{formId:guid}/responses")]
    [ProducesResponseType(typeof(PagedResult<FormResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<FormResponseDto>>> GetResponses(
        Guid formId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFormResponsesQuery(formId, pageNumber, pageSize),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get a single form response by id.
    /// </summary>
    [HttpGet("responses/{responseId:guid}")]
    [ProducesResponseType(typeof(FormResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormResponseDto>> GetResponseById(
        Guid responseId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFormResponseByIdQuery(responseId), cancellationToken);
        return Ok(result);
    }
}

public sealed record SubmitFormResponseRequest(
    IReadOnlyList<FormResponseValueInputDto> Values,
    string? SubmittedBy = null);
