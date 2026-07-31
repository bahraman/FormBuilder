using FluentValidation;
using Vendo.FormBuilder.Application.Common.Validation;

namespace Vendo.FormBuilder.Application.Forms.Commands.CreateFormVersion;

public sealed class CreateFormVersionCommandValidator : AbstractValidator<CreateFormVersionCommand>
{
    public CreateFormVersionCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId).RequiredSubscriberId();
        RuleFor(x => x.RestaurantId).OptionalRestaurantId();
    }
}
