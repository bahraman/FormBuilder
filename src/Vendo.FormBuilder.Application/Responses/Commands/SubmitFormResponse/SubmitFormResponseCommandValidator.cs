using FluentValidation;
using Vendo.FormBuilder.Application.Common.Validation;

namespace Vendo.FormBuilder.Application.Responses.Commands.SubmitFormResponse;

public sealed class SubmitFormResponseCommandValidator : AbstractValidator<SubmitFormResponseCommand>
{
    public SubmitFormResponseCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId).RequiredSubscriberId();
        RuleFor(x => x.Values).NotNull();
        RuleForEach(x => x.Values).ChildRules(value =>
        {
            value.RuleFor(v => v.FieldId).NotEmpty();
        });
    }
}
