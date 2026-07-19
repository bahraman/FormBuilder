using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Enums;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.AddFormField;

public sealed record AddFormFieldCommand(
    long FormId,
    int SubscriberId,
    int? RestaurantId,
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
    string? CreatedBy = null) : IRequest<FormFieldDto>;

public sealed class AddFormFieldCommandHandler : IRequestHandler<AddFormFieldCommand, FormFieldDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddFormFieldCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormFieldDto> Handle(AddFormFieldCommand request, CancellationToken cancellationToken)
    {
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: true,
            cancellationToken);

        var field = form.AddField(
            request.Name,
            request.Label,
            request.FieldType,
            request.DisplayOrder,
            request.IsRequired,
            request.Placeholder,
            request.HelpText,
            request.DefaultValue,
            request.CreatedBy);

        if (request.Options is { Count: > 0 })
        {
            field.ReplaceOptions(request.Options.Select(o =>
                (o.Label, o.Value, o.DisplayOrder, o.IsDefault)));
        }

        if (request.ValidationRules is { Count: > 0 })
        {
            field.ReplaceValidationRules(request.ValidationRules.Select(r =>
                (r.RuleType, r.Value, r.ErrorMessage)));
        }

        _formRepository.AddField(field);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return field.ToDto();
    }
}
