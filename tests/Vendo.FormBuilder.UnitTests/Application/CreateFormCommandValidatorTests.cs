using FluentAssertions;
using Vendo.FormBuilder.Application.Forms.Commands.CreateForm;

namespace Vendo.FormBuilder.UnitTests.Application;

public sealed class CreateFormCommandValidatorTests
{
    private readonly CreateFormCommandValidator _validator = new();
    private const int SubscriberId = 1;

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var command = new CreateFormCommand(SubscriberId, "Contact Form", "Desc", "contact-form");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithoutSubscriberId_ShouldFail()
    {
        var command = new CreateFormCommand(0, "Name", null, "slug");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFormCommand.SubscriberId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bad Slug")]
    [InlineData("slug_with_underscore")]
    public void Validate_WithInvalidSlug_ShouldFail(string slug)
    {
        var command = new CreateFormCommand(SubscriberId, "Name", null, slug);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFormCommand.Slug));
    }
}
