using Vendo.FormBuilder.Application.Common.Models;
using Vendo.FormBuilder.Application.Forms.Commands.ArchiveForm;
using Vendo.FormBuilder.Application.Forms.Commands.CreateForm;
using Vendo.FormBuilder.Application.Forms.Commands.CreateFormVersion;
using Vendo.FormBuilder.Application.Forms.Commands.DeleteForm;
using Vendo.FormBuilder.Application.Forms.Commands.PublishForm;
using Vendo.FormBuilder.Application.Forms.Commands.UpdateForm;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Application.Forms.Queries.GetFormById;
using Vendo.FormBuilder.Application.Forms.Queries.GetForms;
using Vendo.FormBuilder.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Vendo.FormBuilder.Api.Controllers;

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
    /// Get a paginated list of forms for a subscriber (optionally scoped to a restaurant).
    /// When restaurantId is provided, returns that restaurant's forms plus subscriber-level shared forms.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FormSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<FormSummaryDto>>> GetForms(
        [FromQuery] int subscriberId,
        [FromQuery] int? restaurantId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] FormStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFormsQuery(subscriberId, restaurantId, pageNumber, pageSize, search, status),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get a form by id. Requires subscriberId; optional restaurantId enforces restaurant isolation.
    /// </summary>
    [HttpGet("{formId:guid}")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormDetailDto>> GetFormById(
        Guid formId,
        [FromQuery] int subscriberId,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFormByIdQuery(formId, subscriberId, restaurantId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new draft form owned by a subscriber (optionally restaurant-specific).
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
            new CreateFormCommand(
                request.SubscriberId,
                request.RestaurantId,
                request.Name,
                request.Description,
                request.Slug,
                request.CreatedBy),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetFormById),
            new { formId = result.Id, subscriberId = result.SubscriberId, restaurantId = result.RestaurantId },
            result);
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
        [FromQuery] int subscriberId,
        [FromBody] UpdateFormRequest request,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new UpdateFormCommand(
                formId,
                subscriberId,
                restaurantId,
                request.Name,
                request.Description,
                request.RowVersion,
                request.UpdatedBy),
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
        [FromQuery] int subscriberId,
        [FromBody] ActorRequest? request,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new PublishFormCommand(formId, subscriberId, restaurantId, request?.Actor),
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
        [FromQuery] int subscriberId,
        [FromBody] ActorRequest? request,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ArchiveFormCommand(formId, subscriberId, restaurantId, request?.Actor),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new draft version from a published or archived form (same tenant ownership).
    /// </summary>
    [HttpPost("{formId:guid}/versions")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> CreateFormVersion(
        Guid formId,
        [FromQuery] int subscriberId,
        [FromBody] ActorRequest? request,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new CreateFormVersionCommand(formId, subscriberId, restaurantId, request?.Actor),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetFormById),
            new { formId = result.Id, subscriberId = result.SubscriberId, restaurantId = result.RestaurantId },
            result);
    }

    /// <summary>
    /// Soft-delete a form.
    /// </summary>
    [HttpDelete("{formId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteForm(
        Guid formId,
        [FromQuery] int subscriberId,
        [FromQuery] int? restaurantId = null,
        [FromQuery] string? deletedBy = null,
        CancellationToken cancellationToken = default)
    {
        await _sender.Send(
            new DeleteFormCommand(formId, subscriberId, restaurantId, deletedBy),
            cancellationToken);
        return NoContent();
    }
}

public sealed record CreateFormRequest(
    int SubscriberId,
    string Name,
    string? Description,
    string Slug,
    int? RestaurantId = null,
    string? CreatedBy = null);

public sealed record UpdateFormRequest(string Name, string? Description, string RowVersion, string? UpdatedBy = null);
public sealed record ActorRequest(string? Actor = null);
