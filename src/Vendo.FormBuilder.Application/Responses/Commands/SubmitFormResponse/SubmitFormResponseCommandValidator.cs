using FluentValidation;

namespace Vendo.FormBuilder.Application.Responses.Commands.SubmitFormResponse;

public sealed class SubmitFormResponseCommandValidator : AbstractValidator<SubmitFormResponseCommand>
{
    public SubmitFormResponseCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId).NotEmpty().WithMessage("SubscriberId is required.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("RestaurantId cannot be an empty GUID.");
        RuleFor(x => x.Values).NotNull();
        RuleForEach(x => x.Values).ChildRules(value =>
        {
            value.RuleFor(v => v.FieldId).NotEmpty();
        });
    }
}
