using FluentValidation;
using Vendo.FormBuilder.Application.Common.Validation;

namespace Vendo.FormBuilder.Application.Forms.Commands.UpdateFormField;

public sealed class UpdateFormFieldCommandValidator : AbstractValidator<UpdateFormFieldCommand>
{
    public UpdateFormFieldCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.FieldId).NotEmpty();
        RuleFor(x => x.SubscriberId).RequiredSubscriberId();
        RuleFor(x => x.RestaurantId).OptionalRestaurantId();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Placeholder).MaximumLength(500);
        RuleFor(x => x.HelpText).MaximumLength(1000);
        RuleFor(x => x.RowVersion).NotEmpty();

        RuleForEach(x => x.Options).ChildRules(option =>
        {
            option.RuleFor(o => o.Label).NotEmpty().MaximumLength(200);
            option.RuleFor(o => o.Value).NotEmpty().MaximumLength(200);
            option.RuleFor(o => o.DisplayOrder).GreaterThanOrEqualTo(0);
        });

        RuleForEach(x => x.ValidationRules).ChildRules(rule =>
        {
            rule.RuleFor(r => r.RuleType).IsInEnum();
            rule.RuleFor(r => r.Value).NotEmpty().MaximumLength(1000);
            rule.RuleFor(r => r.ErrorMessage).MaximumLength(500);
        });
    }
}
