using FluentValidation;

namespace FormBuilder.Application.Forms.Commands.DeleteForm;

public sealed class DeleteFormCommandValidator : AbstractValidator<DeleteFormCommand>
{
    public DeleteFormCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
    }
}
