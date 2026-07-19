using FluentValidation;

namespace Vendo.FormBuilder.Application.Forms.Commands.CreateForm;

public sealed class CreateFormCommandValidator : AbstractValidator<CreateFormCommand>
{
    public CreateFormCommandValidator()
    {
        RuleFor(x => x.SubscriberId)
            .GreaterThan(0)
            .WithMessage("SubscriberId is required and must be a positive integer.");

        RuleFor(x => x.RestaurantId)
            .Must(id => id is null || id > 0)
            .WithMessage("RestaurantId must be a positive integer when provided. Omit it for subscriber-level forms.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(200)
            .Matches("^[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*$")
            .WithMessage("Slug must be URL-friendly (letters, numbers, and hyphens only).");
    }
}
