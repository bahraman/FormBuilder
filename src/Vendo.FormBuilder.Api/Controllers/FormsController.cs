using Vendo.FormBuilder.Api.Security;
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
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FormSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<FormSummaryDto>>> GetForms(
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        [FromQuery] int? restaurantId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] FormStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new GetFormsQuery(subscriberId, restaurantId, pageNumber, pageSize, search, status),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get a form by id. Optional restaurantId enforces restaurant isolation.
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpGet("{formId:long}")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormDetailDto>> GetFormById(
        long formId,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new GetFormByIdQuery(formId, subscriberId, restaurantId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new draft form owned by a subscriber (optionally restaurant-specific).
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> CreateForm(
        [FromBody] CreateFormRequest request,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        CancellationToken cancellationToken)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new CreateFormCommand(
                subscriberId,
                request.RestaurantId,
                request.Name,
                request.Description,
                request.Slug,
                request.CreatedBy),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetFormById),
            new { formId = result.Id, restaurantId = result.RestaurantId },
            result);
    }

    /// <summary>
    /// Update a draft form's metadata. Requires RowVersion for optimistic concurrency.
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpPut("{formId:long}")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> UpdateForm(
        long formId,
        [FromBody] UpdateFormRequest request,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

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
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpPost("{formId:long}/publish")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> PublishForm(
        long formId,
        [FromBody] ActorRequest? request,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new PublishFormCommand(formId, subscriberId, restaurantId, request?.Actor),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Archive a published form.
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpPost("{formId:long}/archive")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> ArchiveForm(
        long formId,
        [FromBody] ActorRequest? request,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new ArchiveFormCommand(formId, subscriberId, restaurantId, request?.Actor),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new draft version from a published or archived form (same tenant ownership).
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpPost("{formId:long}/versions")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> CreateFormVersion(
        long formId,
        [FromBody] ActorRequest? request,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        [FromQuery] int? restaurantId = null,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new CreateFormVersionCommand(formId, subscriberId, restaurantId, request?.Actor),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetFormById),
            new { formId = result.Id, restaurantId = result.RestaurantId },
            result);
    }

    /// <summary>
    /// Soft-delete a form.
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpDelete("{formId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteForm(
        long formId,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        [FromQuery] int? restaurantId = null,
        [FromQuery] string? deletedBy = null,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        await _sender.Send(
            new DeleteFormCommand(formId, subscriberId, restaurantId, deletedBy),
            cancellationToken);
        return NoContent();
    }
}

public sealed record CreateFormRequest(
    string Name,
    string? Description,
    string Slug,
    int? RestaurantId = null,
    string? CreatedBy = null);

public sealed record UpdateFormRequest(string Name, string? Description, string RowVersion, string? UpdatedBy = null);
public sealed record ActorRequest(string? Actor = null);
