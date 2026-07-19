using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.UpdateForm;

public sealed class UpdateFormCommandValidator : AbstractValidator<UpdateFormCommand>
{
    public UpdateFormCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId)
            .GreaterThan(0)
            .WithMessage("SubscriberId is required and must be a positive integer.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id > 0)
            .WithMessage("RestaurantId must be a positive integer when provided.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}
