using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.UpdateFormField;

public sealed record UpdateFormFieldCommand(
    long FormId,
    long FieldId,
    int SubscriberId,
    string Label,
    bool IsRequired,
    string? Placeholder,
    string? HelpText,
    string? DefaultValue,
    string RowVersion,
    IReadOnlyList<FieldOptionInputDto>? Options = null,
    IReadOnlyList<FieldValidationRuleInputDto>? ValidationRules = null,
    string? UpdatedBy = null) : IRequest<FormFieldDto>;

public sealed class UpdateFormFieldCommandHandler : IRequestHandler<UpdateFormFieldCommand, FormFieldDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFormFieldCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormFieldDto> Handle(UpdateFormFieldCommand request, CancellationToken cancellationToken)
    {
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            withDetails: true,
            cancellationToken);

        if (form.Status != Domain.Enums.FormStatus.Draft)
        {
            throw new ConflictException("Only draft forms can be modified. Create a new version to make changes.");
        }

        var field = form.GetField(request.FieldId);
        _formRepository.SetOriginalRowVersion(field, Convert.FromBase64String(request.RowVersion));
        field.Update(
            request.Label,
            request.IsRequired,
            request.Placeholder,
            request.HelpText,
            request.DefaultValue,
            request.UpdatedBy);

        if (request.Options is not null)
        {
            field.ReplaceOptions(request.Options.Select(o =>
                (o.Label, o.Value, o.DisplayOrder, o.IsDefault)));
        }

        if (request.ValidationRules is not null)
        {
            field.ReplaceValidationRules(request.ValidationRules.Select(r =>
                (r.RuleType, r.Value, r.ErrorMessage)));
        }

        form.UpdatedAtUtc = DateTime.UtcNow;
        form.UpdatedBy = request.UpdatedBy;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return field.ToDto();
    }
}
