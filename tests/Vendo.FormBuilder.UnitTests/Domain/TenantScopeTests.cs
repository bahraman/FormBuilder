using FluentAssertions;
using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Exceptions;

namespace Vendo.FormBuilder.UnitTests.Domain;

public sealed class TenantScopeTests
{
    private const int SubscriberA = 1;
    private const int SubscriberB = 2;

    [Fact]
    public void CanAccess_SameSubscriber_ShouldBeTrue()
    {
        var scope = TenantScope.ForSubscriber(SubscriberA);

        scope.CanAccess(SubscriberA).Should().BeTrue();
    }

    [Fact]
    public void CanAccess_OtherSubscriber_ShouldBeFalse()
    {
        var scope = TenantScope.ForSubscriber(SubscriberA);

        scope.CanAccess(SubscriberB).Should().BeFalse();
    }

    [Fact]
    public void ForSubscriber_WithNonPositive_ShouldThrow()
    {
        var act = () => TenantScope.ForSubscriber(0);

        act.Should().Throw<DomainException>().WithMessage("*SubscriberId*");
    }

    [Fact]
    public void EnsureCanAccess_OtherSubscriber_ShouldThrowNotFound()
    {
        var scope = TenantScope.ForSubscriber(SubscriberA);

        var act = () => scope.EnsureCanAccess(SubscriberB);

        act.Should().Throw<NotFoundException>();
    }
}
