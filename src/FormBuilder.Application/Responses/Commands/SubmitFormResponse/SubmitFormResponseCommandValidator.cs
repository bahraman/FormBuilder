using FluentValidation;

namespace FormBuilder.Application.Responses.Commands.SubmitFormResponse;

public sealed class SubmitFormResponseCommandValidator : AbstractValidator<SubmitFormResponseCommand>
{
    public SubmitFormResponseCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.Values).NotNull();
        RuleForEach(x => x.Values).ChildRules(value =>
        {
            value.RuleFor(v => v.FieldId).NotEmpty();
        });
    }
}
