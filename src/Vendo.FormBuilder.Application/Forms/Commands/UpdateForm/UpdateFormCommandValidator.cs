using FluentValidation;
using Vendo.FormBuilder.Application.Common.Validation;

namespace Vendo.FormBuilder.Application.Forms.Commands.UpdateForm;

public sealed class UpdateFormCommandValidator : AbstractValidator<UpdateFormCommand>
{
    public UpdateFormCommandValidator()
    {
        RuleFor(x => x.FormId).NotEmpty();
        RuleFor(x => x.SubscriberId).RequiredSubscriberId();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}
