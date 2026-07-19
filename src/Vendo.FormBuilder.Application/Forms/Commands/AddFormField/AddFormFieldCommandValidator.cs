using FluentValidation;
using Vendo.FormBuilder.Domain.Enums;

namespace Vendo.FormBuilder.Application.Forms.Commands.AddFormField;

public sealed class AddFormFieldCommandValidator : AbstractValidator<AddFormFieldCommand>
{
    public AddFormFieldCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId)
            .GreaterThan(0)
            .WithMessage("SubscriberId is required and must be a positive integer.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id > 0)
            .WithMessage("RestaurantId must be a positive integer when provided.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .Matches("^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Field name must start with a letter and contain only letters, numbers, and underscores.");
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldType).IsInEnum();
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Placeholder).MaximumLength(500);
        RuleFor(x => x.HelpText).MaximumLength(1000);

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

        RuleFor(x => x)
            .Must(x => x.Options is { Count: > 0 } ||
                       x.FieldType is not (FieldType.RadioButton or FieldType.Dropdown or FieldType.MultiSelect))
            .WithMessage("RadioButton, Dropdown, and MultiSelect fields require at least one option.");
    }
}
