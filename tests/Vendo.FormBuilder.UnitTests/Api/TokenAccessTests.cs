using FluentAssertions;
using Vendo.FormBuilder.Api.Security;

namespace Vendo.FormBuilder.UnitTests.Api;

public sealed class TokenAccessTests
{
    [Fact]
    public void HasAccess_AdminRole_ShouldAllowAnySubscriber()
    {
        TokenAccess.HasAccess(TokenAccess.AdminRoleId, subscriberId: 999, subscriberIds: "[]")
            .Should().BeTrue();
    }

    [Fact]
    public void HasAccess_SubscriberInList_ShouldAllow()
    {
        TokenAccess.HasAccess(roleId: 1, subscriberId: 42, subscriberIds: "[10,42,77]")
            .Should().BeTrue();
    }

    [Fact]
    public void HasAccess_SubscriberNotInList_ShouldDeny()
    {
        TokenAccess.HasAccess(roleId: 1, subscriberId: 42, subscriberIds: "[10,77]")
            .Should().BeFalse();
    }

    [Fact]
    public void HasAccess_NullSubscriberIds_ShouldDenyNonAdmin()
    {
        TokenAccess.HasAccess(roleId: 1, subscriberId: 42, subscriberIds: null)
            .Should().BeFalse();
    }

    [Fact]
    public void HasAccess_InvalidJson_ShouldDeny()
    {
        TokenAccess.HasAccess(roleId: 1, subscriberId: 42, subscriberIds: "not-json")
            .Should().BeFalse();
    }
}
