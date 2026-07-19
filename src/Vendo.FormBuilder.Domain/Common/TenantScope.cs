using Vendo.FormBuilder.Domain.Exceptions;

namespace Vendo.FormBuilder.Domain.Common;

/// <summary>
/// Multi-tenant ownership scope for forms.
/// SubscriberId is always required. RestaurantId is optional:
/// null = subscriber-level (shared across restaurants); set = restaurant-specific.
/// </summary>
public readonly record struct TenantScope(Guid SubscriberId, Guid? RestaurantId)
{
    public static TenantScope ForSubscriber(Guid subscriberId, Guid? restaurantId = null)
    {
        if (subscriberId == Guid.Empty)
        {
            throw new DomainException("SubscriberId is required.");
        }

        if (restaurantId == Guid.Empty)
        {
            throw new DomainException("RestaurantId cannot be an empty GUID. Omit it for subscriber-level forms.");
        }

        return new TenantScope(subscriberId, restaurantId);
    }

    /// <summary>
    /// Returns true when this scope may access a form owned by the given tenant.
    /// Subscriber mismatch is never allowed. Restaurant-specific forms are only
    /// visible to that restaurant (or to a subscriber-wide request with no restaurant filter).
    /// </summary>
    public bool CanAccess(Guid formSubscriberId, Guid? formRestaurantId)
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

    public void EnsureCanAccess(Guid formSubscriberId, Guid? formRestaurantId)
    {
        if (!CanAccess(formSubscriberId, formRestaurantId))
        {
            throw new NotFoundException("Form", "not found for the specified tenant scope.");
        }
    }
}
