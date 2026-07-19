using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.ReorderFormFields;

public sealed class ReorderFormFieldsCommandValidator : AbstractValidator<ReorderFormFieldsCommand>
{
    public ReorderFormFieldsCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId)
            .GreaterThan(0)
            .WithMessage("SubscriberId is required and must be a positive integer.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id > 0)
            .WithMessage("RestaurantId must be a positive integer when provided.");
        RuleFor(x => x.FieldOrders).NotEmpty();
        RuleForEach(x => x.FieldOrders).ChildRules(item =>
        {
            item.RuleFor(i => i.FieldId).NotEmpty();
            item.RuleFor(i => i.DisplayOrder).GreaterThanOrEqualTo(0);
        });
    }
}
