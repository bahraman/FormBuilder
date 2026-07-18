using FormBuilder.Application.Forms.Commands.AddFormField;
using FormBuilder.Application.Forms.Commands.DeleteFormField;
using FormBuilder.Application.Forms.Commands.ReorderFormFields;
using FormBuilder.Application.Forms.Commands.UpdateFormField;
using FormBuilder.Application.Forms.Dtos;
using FormBuilder.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Api.Controllers;

[ApiController]
[Route("api/forms/{formId:guid}/fields")]
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
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FormFieldDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormFieldDto>> AddField(
        Guid formId,
        [FromBody] AddFormFieldRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddFormFieldCommand(
                formId,
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
    /// </summary>
    [HttpPut("{fieldId:guid}")]
    [ProducesResponseType(typeof(FormFieldDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormFieldDto>> UpdateField(
        Guid formId,
        Guid fieldId,
        [FromBody] UpdateFormFieldRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateFormFieldCommand(
                formId,
                fieldId,
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
    /// </summary>
    [HttpDelete("{fieldId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteField(
        Guid formId,
        Guid fieldId,
        [FromQuery] string? deletedBy = null,
        CancellationToken cancellationToken = default)
    {
        await _sender.Send(new DeleteFormFieldCommand(formId, fieldId, deletedBy), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reorder fields on a draft form.
    /// </summary>
    [HttpPut("reorder")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FormDetailDto>> ReorderFields(
        Guid formId,
        [FromBody] ReorderFormFieldsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReorderFormFieldsCommand(formId, request.FieldOrders, request.UpdatedBy),
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
