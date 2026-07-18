using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.DeleteFormField;

public sealed class DeleteFormFieldCommandValidator : AbstractValidator<DeleteFormFieldCommand>
{
    public DeleteFormFieldCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.FieldId).NotEmpty();
    }
}
