using FluentAssertions;
using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Exceptions;

namespace Vendo.FormBuilder.UnitTests.Domain;

public sealed class TenantScopeTests
{
    private const int SubscriberA = 1;
    private const int Restaurant1 = 10;
    private const int Restaurant2 = 20;

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
    public void ForSubscriber_WithZeroRestaurant_ShouldTreatAsSubscriberLevel()
    {
        var scope = TenantScope.ForSubscriber(SubscriberA, 0);

        scope.RestaurantId.Should().BeNull();
    }

    [Fact]
    public void ForSubscriber_WithNegativeRestaurant_ShouldThrow()
    {
        var act = () => TenantScope.ForSubscriber(SubscriberA, -1);

        act.Should().Throw<DomainException>();
    }
}
