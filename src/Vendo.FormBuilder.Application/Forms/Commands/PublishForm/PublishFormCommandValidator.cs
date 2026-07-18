using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.PublishForm;

public sealed class PublishFormCommandValidator : AbstractValidator<PublishFormCommand>
{
    public PublishFormCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
    }
}
