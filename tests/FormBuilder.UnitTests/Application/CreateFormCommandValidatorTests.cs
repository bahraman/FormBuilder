using FluentAssertions;
using FormBuilder.Application.Forms.Commands.CreateForm;

namespace FormBuilder.UnitTests.Application;

public sealed class CreateFormCommandValidatorTests
{
    private readonly CreateFormCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var command = new CreateFormCommand("Contact Form", "Desc", "contact-form");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bad Slug")]
    [InlineData("slug_with_underscore")]
    public void Validate_WithInvalidSlug_ShouldFail(string slug)
    {
        var command = new CreateFormCommand("Name", null, slug);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFormCommand.Slug));
    }
}
