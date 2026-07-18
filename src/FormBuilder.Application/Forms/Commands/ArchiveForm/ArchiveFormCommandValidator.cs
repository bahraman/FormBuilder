using FluentValidation;

namespace FormBuilder.Application.Forms.Commands.ArchiveForm;

public sealed class ArchiveFormCommandValidator : AbstractValidator<ArchiveFormCommand>
{
    public ArchiveFormCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
    }
}
