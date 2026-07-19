using FluentValidation;

namespace Vendo.FormBuilder.Application.Responses.Commands.SubmitFormResponse;

public sealed class SubmitFormResponseCommandValidator : AbstractValidator<SubmitFormResponseCommand>
{
    public SubmitFormResponseCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId)
            .GreaterThan(0)
            .WithMessage("SubscriberId is required and must be a positive integer.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id > 0)
            .WithMessage("RestaurantId must be a positive integer when provided.");
        RuleFor(x => x.Values).NotNull();
        RuleForEach(x => x.Values).ChildRules(value =>
        {
            value.RuleFor(v => v.FieldId).NotEmpty();
        });
    }
}
