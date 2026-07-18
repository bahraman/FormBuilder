using FluentValidation;

namespace FormBuilder.Application.Forms.Commands.CreateFormVersion;

public sealed class CreateFormVersionCommandValidator : AbstractValidator<CreateFormVersionCommand>
{
    public CreateFormVersionCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
    }
}
