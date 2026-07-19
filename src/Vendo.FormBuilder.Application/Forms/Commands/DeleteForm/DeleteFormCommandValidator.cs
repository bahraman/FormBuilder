using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.DeleteForm;

public sealed class DeleteFormCommandValidator : AbstractValidator<DeleteFormCommand>
{
    public DeleteFormCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId).NotEmpty().WithMessage("SubscriberId is required.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("RestaurantId cannot be an empty GUID.");
    }
}
