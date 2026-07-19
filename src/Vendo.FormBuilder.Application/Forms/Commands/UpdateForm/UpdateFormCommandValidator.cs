using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.UpdateForm;

public sealed class UpdateFormCommandValidator : AbstractValidator<UpdateFormCommand>
{
    public UpdateFormCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId).NotEmpty().WithMessage("SubscriberId is required.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("RestaurantId cannot be an empty GUID.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}
