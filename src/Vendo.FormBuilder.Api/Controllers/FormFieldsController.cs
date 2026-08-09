using Vendo.FormBuilder.Api.Security;
using Vendo.FormBuilder.Application.Forms.Commands.AddFormField;
using Vendo.FormBuilder.Application.Forms.Commands.DeleteFormField;
using Vendo.FormBuilder.Application.Forms.Commands.ReorderFormFields;
using Vendo.FormBuilder.Application.Forms.Commands.UpdateFormField;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Vendo.FormBuilder.Api.Controllers;

[ApiController]
[Route("api/forms/{formId:long}/fields")]
[Produces("application/json")]
public sealed class FormFieldsController : ControllerBase
{
    private readonly ISender _sender;

    public FormFieldsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Add a field to a draft form.
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FormFieldDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormFieldDto>> AddField(
        long formId,
        [FromBody] AddFormFieldRequest request,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new AddFormFieldCommand(
                formId,
                subscriberId,
                request.Name,
                request.Label,
                request.FieldType,
                request.DisplayOrder,
                request.IsRequired,
                request.Placeholder,
                request.HelpText,
                request.DefaultValue,
                request.Options,
                request.ValidationRules,
                request.CreatedBy),
            cancellationToken);

        return CreatedAtAction(
            nameof(FormsController.GetFormById),
            "Forms",
            new { formId },
            result);
    }

    /// <summary>
    /// Update a field on a draft form.
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpPut("{fieldId:long}")]
    [ProducesResponseType(typeof(FormFieldDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormFieldDto>> UpdateField(
        long formId,
        long fieldId,
        [FromBody] UpdateFormFieldRequest request,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new UpdateFormFieldCommand(
                formId,
                fieldId,
                subscriberId,
                request.Label,
                request.IsRequired,
                request.Placeholder,
                request.HelpText,
                request.DefaultValue,
                request.RowVersion,
                request.Options,
                request.ValidationRules,
                request.UpdatedBy),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Soft-delete a field from a draft form.
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpDelete("{fieldId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteField(
        long formId,
        long fieldId,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        [FromQuery] string? deletedBy = null,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        await _sender.Send(
            new DeleteFormFieldCommand(formId, fieldId, subscriberId, deletedBy),
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reorder fields on a draft form.
    /// Identity from headers: x-user-id, x-role-id, x-subscriber-id, x-subscriber-ids.
    /// </summary>
    [HttpPut("reorder")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> ReorderFields(
        long formId,
        [FromBody] ReorderFormFieldsRequest request,
        [FromHeader(Name = AdminFormHeaders.UserId)] int userId,
        [FromHeader(Name = AdminFormHeaders.SubscriberId)] int subscriberId,
        [FromHeader(Name = AdminFormHeaders.SubscriberIds)] string? subscriberIds,
        [FromHeader(Name = AdminFormHeaders.RoleId)] int roleId,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        if (AdminFormHeaders.UnauthorizedIfNoAccess(this, roleId, subscriberId, subscriberIds) is { } unauthorized)
        {
            return unauthorized;
        }

        var result = await _sender.Send(
            new ReorderFormFieldsCommand(
                formId,
                subscriberId,
                request.FieldOrders,
                request.UpdatedBy),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record AddFormFieldRequest(
    string Name,
    string Label,
    FieldType FieldType,
    int DisplayOrder,
    bool IsRequired = false,
    string? Placeholder = null,
    string? HelpText = null,
    string? DefaultValue = null,
    IReadOnlyList<FieldOptionInputDto>? Options = null,
    IReadOnlyList<FieldValidationRuleInputDto>? ValidationRules = null,
    string? CreatedBy = null);

public sealed record UpdateFormFieldRequest(
    string Label,
    bool IsRequired,
    string? Placeholder,
    string? HelpText,
    string? DefaultValue,
    string RowVersion,
    IReadOnlyList<FieldOptionInputDto>? Options = null,
    IReadOnlyList<FieldValidationRuleInputDto>? ValidationRules = null,
    string? UpdatedBy = null);

public sealed record ReorderFormFieldsRequest(
    IReadOnlyList<FieldOrderItemDto> FieldOrders,
    string? UpdatedBy = null);
