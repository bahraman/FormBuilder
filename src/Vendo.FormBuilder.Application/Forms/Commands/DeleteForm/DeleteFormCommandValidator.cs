using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.DeleteForm;

public sealed class DeleteFormCommandValidator : AbstractValidator<DeleteFormCommand>
{
    public DeleteFormCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId)
            .GreaterThan(0)
            .WithMessage("SubscriberId is required and must be a positive integer.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id > 0)
            .WithMessage("RestaurantId must be a positive integer when provided.");
    }
}
