using System.Text.Json;

namespace Vendo.FormBuilder.Api.Security;

/// <summary>
/// Gateway header token check for admin form APIs.
/// Admin role (1013) bypasses subscriber membership; otherwise the target subscriber must appear in x-subscriber-ids.
/// </summary>
public static class TokenAccess
{
    public const int AdminRoleId = 1013;

    public static bool HasAccess(int roleId, long subscriberId, string? subscriberIds)
    {
        List<long> ids;
        try
        {
            ids = JsonSerializer.Deserialize<List<long>>(subscriberIds ?? "[]") ?? [];
        }
        catch (JsonException)
        {
            return false;
        }

        var isAdmin = roleId == AdminRoleId;
        var hasSubscriberAccess = ids.Contains(subscriberId);

        return isAdmin || hasSubscriberAccess;
    }
}
