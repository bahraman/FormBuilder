using FluentAssertions;
using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Enums;
using Vendo.FormBuilder.Domain.Exceptions;

namespace Vendo.FormBuilder.UnitTests.Domain;

public sealed class FormTests
{
    private static readonly Guid SubscriberA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SubscriberB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Restaurant1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid Restaurant2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void Create_ShouldRequireSubscriberAndAllowNullRestaurant()
    {
        var form = Form.Create(SubscriberA, null, "Contact Us", "Collect inquiries", "contact-us", "admin");

        form.SubscriberId.Should().Be(SubscriberA);
        form.RestaurantId.Should().BeNull();
        form.IsSubscriberLevel.Should().BeTrue();
        form.Name.Should().Be("Contact Us");
        form.Slug.Should().Be("contact-us");
        form.Status.Should().Be(FormStatus.Draft);
        form.Version.Should().Be(1);
    }

    [Fact]
    public void Create_WithRestaurant_ShouldBeRestaurantSpecific()
    {
        var form = Form.Create(SubscriberA, Restaurant1, "Local Menu", null, "local-menu");

        form.RestaurantId.Should().Be(Restaurant1);
        form.IsSubscriberLevel.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptySubscriber_ShouldThrow()
    {
        var act = () => Form.Create(Guid.Empty, null, "Name", null, "slug");

        act.Should().Throw<DomainException>().WithMessage("*SubscriberId*");
    }

    [Fact]
    public void EnsureAccessibleTo_ShouldBlockOtherSubscriber()
    {
        var form = Form.Create(SubscriberA, null, "Survey", null, "survey");
        var otherSubscriber = TenantScope.ForSubscriber(SubscriberB);

        var act = () => form.EnsureAccessibleTo(otherSubscriber);

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void EnsureAccessibleTo_ShouldBlockOtherRestaurant()
    {
        var form = Form.Create(SubscriberA, Restaurant1, "Survey", null, "survey");
        var otherRestaurant = TenantScope.ForSubscriber(SubscriberA, Restaurant2);

        var act = () => form.EnsureAccessibleTo(otherRestaurant);

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void EnsureAccessibleTo_ShouldAllowSharedFormForAnyRestaurantOfSubscriber()
    {
        var form = Form.Create(SubscriberA, null, "Shared", null, "shared");
        var restaurantScope = TenantScope.ForSubscriber(SubscriberA, Restaurant1);

        var act = () => form.EnsureAccessibleTo(restaurantScope);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureAccessibleTo_SubscriberWide_ShouldAccessRestaurantForm()
    {
        var form = Form.Create(SubscriberA, Restaurant1, "Local", null, "local");
        var subscriberWide = TenantScope.ForSubscriber(SubscriberA);

        var act = () => form.EnsureAccessibleTo(subscriberWide);

        act.Should().NotThrow();
    }

    [Fact]
    public void Publish_WithoutFields_ShouldThrow()
    {
        var form = Form.Create(SubscriberA, null, "Empty", null, "empty");

        var act = () => form.Publish();

        act.Should().Throw<DomainException>()
            .WithMessage("*at least one field*");
    }

    [Fact]
    public void Publish_WithFields_ShouldSucceed()
    {
        var form = Form.Create(SubscriberA, Restaurant1, "Survey", null, "survey");
        form.AddField("email", "Email", FieldType.Email, 0, isRequired: true);

        form.Publish("admin");

        form.Status.Should().Be(FormStatus.Published);
        form.PublishedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Update_WhenPublished_ShouldThrow()
    {
        var form = Form.Create(SubscriberA, null, "Survey", null, "survey");
        form.AddField("name", "Name", FieldType.Text, 0);
        form.Publish();

        var act = () => form.Update("New Name", null);

        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void CreateNewVersion_ShouldPreserveTenantOwnership()
    {
        var form = Form.Create(SubscriberA, Restaurant1, "Survey", "Desc", "survey");
        var field = form.AddField("rating", "Rating", FieldType.Dropdown, 0, isRequired: true);
        field.AddOption("Good", "good", 0, true);
        field.AddOption("Bad", "bad", 1);
        form.Publish();

        var next = form.CreateNewVersion("editor");

        next.SubscriberId.Should().Be(SubscriberA);
        next.RestaurantId.Should().Be(Restaurant1);
        next.Version.Should().Be(2);
        next.Status.Should().Be(FormStatus.Draft);
        next.ParentFormId.Should().Be(form.Id);
        next.Fields.Should().HaveCount(1);
    }

    [Fact]
    public void ReorderFields_ShouldUpdateDisplayOrder()
    {
        var form = Form.Create(SubscriberA, null, "Survey", null, "survey");
        var first = form.AddField("a", "A", FieldType.Text, 0);
        var second = form.AddField("b", "B", FieldType.Text, 1);

        form.ReorderFields(new Dictionary<Guid, int>
        {
            [first.Id] = 5,
            [second.Id] = 1
        });

        form.GetField(first.Id).DisplayOrder.Should().Be(5);
        form.GetField(second.Id).DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void SoftDelete_ShouldMarkFormAndFieldsDeleted()
    {
        var form = Form.Create(SubscriberA, null, "Survey", null, "survey");
        form.AddField("a", "A", FieldType.Text, 0);

        form.SoftDelete("admin");

        form.IsDeleted.Should().BeTrue();
        form.Fields.Should().OnlyContain(f => f.IsDeleted);
    }
}
