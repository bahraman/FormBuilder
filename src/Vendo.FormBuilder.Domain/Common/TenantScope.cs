using Vendo.FormBuilder.Domain.Exceptions;

namespace Vendo.FormBuilder.Domain.Common;

/// <summary>
/// Multi-tenant ownership scope for forms.
/// SubscriberId is always required. RestaurantId is optional:
/// null (or 0) = subscriber-level (shared across restaurants); positive = restaurant-specific.
/// </summary>
public readonly record struct TenantScope(int SubscriberId, int? RestaurantId)
{
    public static TenantScope ForSubscriber(int subscriberId, int? restaurantId = null)
    {
        if (subscriberId <= 0)
        {
            throw new DomainException("SubscriberId is required and must be a positive integer.");
        }

        // 0 is treated as "not set" (subscriber-level), matching common API clients.
        restaurantId = NormalizeRestaurantId(restaurantId);

        if (restaurantId is < 0)
        {
            throw new DomainException("RestaurantId must be a positive integer when provided. Use null or 0 for subscriber-level forms.");
        }

        return new TenantScope(subscriberId, restaurantId);
    }

    /// <summary>Maps 0 to null so clients can send restaurantId: 0 for subscriber-level forms.</summary>
    public static int? NormalizeRestaurantId(int? restaurantId) =>
        restaurantId is null or 0 ? null : restaurantId;

    /// <summary>
    /// Returns true when this scope may access a form owned by the given tenant.
    /// Subscriber mismatch is never allowed. Restaurant-specific forms are only
    /// visible to that restaurant (or to a subscriber-wide request with no restaurant filter).
    /// </summary>
    public bool CanAccess(int formSubscriberId, int? formRestaurantId)
    {
        if (formSubscriberId != SubscriberId)
        {
            return false;
        }

        // Subscriber-wide request (no restaurant filter) can access all forms of that subscriber.
        if (RestaurantId is null)
        {
            return true;
        }

        // Restaurant request: shared (null) forms + that restaurant's forms only.
        return formRestaurantId is null || formRestaurantId == RestaurantId;
    }

    public void EnsureCanAccess(int formSubscriberId, int? formRestaurantId)
    {
        if (!CanAccess(formSubscriberId, formRestaurantId))
        {
            throw new NotFoundException("Form", "not found for the specified tenant scope.");
        }
    }
}
