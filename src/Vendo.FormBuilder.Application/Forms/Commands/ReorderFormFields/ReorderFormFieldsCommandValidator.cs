using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.ReorderFormFields;

public sealed class ReorderFormFieldsCommandValidator : AbstractValidator<ReorderFormFieldsCommand>
{
    public ReorderFormFieldsCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId).NotEmpty().WithMessage("SubscriberId is required.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("RestaurantId cannot be an empty GUID.");
        RuleFor(x => x.FieldOrders).NotEmpty();
        RuleForEach(x => x.FieldOrders).ChildRules(item =>
        {
            item.RuleFor(i => i.FieldId).NotEmpty();
            item.RuleFor(i => i.DisplayOrder).GreaterThanOrEqualTo(0);
        });
    }
}
