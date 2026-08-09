using Vendo.FormBuilder.Domain.Exceptions;

namespace Vendo.FormBuilder.Domain.Common;

/// <summary>
/// Multi-tenant ownership scope for forms.
/// SubscriberId is always required; forms are filtered by subscriber only.
/// </summary>
public readonly record struct TenantScope(int SubscriberId)
{
    public static TenantScope ForSubscriber(int subscriberId)
    {
        if (subscriberId <= 0)
        {
            throw new DomainException("SubscriberId is required and must be a positive integer.");
        }

        return new TenantScope(subscriberId);
    }

    public bool CanAccess(int formSubscriberId) => formSubscriberId == SubscriberId;

    public void EnsureCanAccess(int formSubscriberId)
    {
        if (!CanAccess(formSubscriberId))
        {
            throw new NotFoundException("Form", "not found for the specified tenant scope.");
        }
    }
}
