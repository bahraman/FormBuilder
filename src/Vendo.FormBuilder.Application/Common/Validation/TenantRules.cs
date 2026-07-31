using FluentValidation;

namespace Vendo.FormBuilder.Application.Common.Validation;

internal static class TenantRules
{
    /// <summary>
    /// RestaurantId is optional. null or 0 = subscriber-level; positive = restaurant-specific.
    /// </summary>
    public static IRuleBuilderOptions<T, int?> OptionalRestaurantId<T>(this IRuleBuilder<T, int?> ruleBuilder) =>
        ruleBuilder
            .Must(id => id is null || id >= 0)
            .WithMessage("RestaurantId must be null, 0 (subscriber-level), or a positive integer.");

    public static IRuleBuilderOptions<T, int> RequiredSubscriberId<T>(this IRuleBuilder<T, int> ruleBuilder) =>
        ruleBuilder
            .GreaterThan(0)
            .WithMessage("SubscriberId is required and must be a positive integer.");
}
