using Microsoft.AspNetCore.Mvc;

namespace Vendo.FormBuilder.Api.Security;

/// <summary>
/// Shared gateway header names for admin form-builder APIs.
/// Target subscriberId comes from the route (or request model), not headers.
/// </summary>
public static class AdminFormHeaders
{
    public const string UserId = "x-user-id";
    public const string RoleId = "x-role-id";
    public const string SubscriberIds = "x-subscriber-ids";

    public static UnauthorizedObjectResult? UnauthorizedIfNoAccess(
        ControllerBase controller,
        int roleId,
        int subscriberId,
        string? subscriberIds)
    {
        if (!TokenAccess.HasAccess(roleId, subscriberId, subscriberIds))
        {
            return controller.Unauthorized("Invalid token");
        }

        return null;
    }
}
