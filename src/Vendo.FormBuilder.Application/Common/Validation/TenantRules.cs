using FluentValidation;

namespace Vendo.FormBuilder.Application.Common.Validation;

internal static class TenantRules
{
    public static IRuleBuilderOptions<T, int> RequiredSubscriberId<T>(this IRuleBuilder<T, int> ruleBuilder) =>
        ruleBuilder
            .GreaterThan(0)
            .WithMessage("SubscriberId is required and must be a positive integer.");
}
