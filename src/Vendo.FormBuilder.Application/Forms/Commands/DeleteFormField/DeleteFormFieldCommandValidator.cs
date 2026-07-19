using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.DeleteFormField;

public sealed class DeleteFormFieldCommandValidator : AbstractValidator<DeleteFormFieldCommand>
{
    public DeleteFormFieldCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.FieldId).NotEmpty();
        RuleFor(x => x.SubscriberId).NotEmpty().WithMessage("SubscriberId is required.");
        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("RestaurantId cannot be an empty GUID.");
    }
}
