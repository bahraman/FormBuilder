using FluentAssertions;
using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Exceptions;

namespace Vendo.FormBuilder.UnitTests.Domain;

public sealed class TenantScopeTests
{
    private static readonly Guid SubscriberA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Restaurant1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid Restaurant2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void CanAccess_SharedForm_FromRestaurantScope_ShouldBeTrue()
    {
        var scope = TenantScope.ForSubscriber(SubscriberA, Restaurant1);

        scope.CanAccess(SubscriberA, null).Should().BeTrue();
    }

    [Fact]
    public void CanAccess_OtherRestaurantForm_ShouldBeFalse()
    {
        var scope = TenantScope.ForSubscriber(SubscriberA, Restaurant1);

        scope.CanAccess(SubscriberA, Restaurant2).Should().BeFalse();
    }

    [Fact]
    public void ForSubscriber_WithEmptyRestaurant_ShouldThrow()
    {
        var act = () => TenantScope.ForSubscriber(SubscriberA, Guid.Empty);

        act.Should().Throw<DomainException>();
    }
}
