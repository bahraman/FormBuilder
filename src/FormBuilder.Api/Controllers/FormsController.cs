using FormBuilder.Application.Common.Models;
using FormBuilder.Application.Forms.Commands.ArchiveForm;
using FormBuilder.Application.Forms.Commands.CreateForm;
using FormBuilder.Application.Forms.Commands.CreateFormVersion;
using FormBuilder.Application.Forms.Commands.DeleteForm;
using FormBuilder.Application.Forms.Commands.PublishForm;
using FormBuilder.Application.Forms.Commands.UpdateForm;
using FormBuilder.Application.Forms.Dtos;
using FormBuilder.Application.Forms.Queries.GetFormById;
using FormBuilder.Application.Forms.Queries.GetForms;
using FormBuilder.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Api.Controllers;

[ApiController]
[Route("api/forms")]
[Produces("application/json")]
public sealed class FormsController : ControllerBase
{
    private readonly ISender _sender;

    public FormsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get a paginated list of forms with optional search and status filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FormSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FormSummaryDto>>> GetForms(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] FormStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFormsQuery(pageNumber, pageSize, search, status),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get a form by id including fields, options, and validation rules.
    /// </summary>
    [HttpGet("{formId:guid}")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormDetailDto>> GetFormById(
        Guid formId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFormByIdQuery(formId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new draft form.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> CreateForm(
        [FromBody] CreateFormRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateFormCommand(request.Name, request.Description, request.Slug, request.CreatedBy),
            cancellationToken);

        return CreatedAtAction(nameof(GetFormById), new { formId = result.Id }, result);
    }

    /// <summary>
    /// Update a draft form's metadata. Requires RowVersion for optimistic concurrency.
    /// </summary>
    [HttpPut("{formId:guid}")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> UpdateForm(
        Guid formId,
        [FromBody] UpdateFormRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateFormCommand(formId, request.Name, request.Description, request.RowVersion, request.UpdatedBy),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Publish a draft form so it can accept responses.
    /// </summary>
    [HttpPost("{formId:guid}/publish")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> PublishForm(
        Guid formId,
        [FromBody] ActorRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new PublishFormCommand(formId, request?.Actor),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Archive a published form.
    /// </summary>
    [HttpPost("{formId:guid}/archive")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> ArchiveForm(
        Guid formId,
        [FromBody] ActorRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ArchiveFormCommand(formId, request?.Actor),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new draft version from a published or archived form.
    /// </summary>
    [HttpPost("{formId:guid}/versions")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> CreateFormVersion(
        Guid formId,
        [FromBody] ActorRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateFormVersionCommand(formId, request?.Actor),
            cancellationToken);

        return CreatedAtAction(nameof(GetFormById), new { formId = result.Id }, result);
    }

    /// <summary>
    /// Soft-delete a form.
    /// </summary>
    [HttpDelete("{formId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteForm(
        Guid formId,
        [FromQuery] string? deletedBy = null,
        CancellationToken cancellationToken = default)
    {
        await _sender.Send(new DeleteFormCommand(formId, deletedBy), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateFormRequest(string Name, string? Description, string Slug, string? CreatedBy = null);
public sealed record UpdateFormRequest(string Name, string? Description, string RowVersion, string? UpdatedBy = null);
public sealed record ActorRequest(string? Actor = null);
