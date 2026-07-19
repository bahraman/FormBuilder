using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Enums;
using Vendo.FormBuilder.Infrastructure.Persistence;

namespace Vendo.FormBuilder.UnitTests.Infrastructure;

public sealed class TemporaryLongKeyTests
{
    [Fact]
    public void PrepareForSave_marks_negative_form_id_as_temporary()
    {
        using var db = CreateContext();
        var form = Form.Create(1, null, "Menu Survey", null, "menu-survey");
        form.Id.Should().BeNegative();

        db.Forms.Add(form);
        InvokePrepareForSave(db);

        db.Entry(form).Property(x => x.Id).IsTemporary.Should().BeTrue(
            "negative LongEntity Ids must be temporary so SQL IDENTITY generates the real key");
    }

    [Fact]
    public void PrepareForSave_marks_negative_ids_across_form_field_graph()
    {
        using var db = CreateContext();

        var form = Form.Create(1, null, "Feedback", null, "feedback");
        var field = form.AddField("rating", "Rating", FieldType.Dropdown, 0, isRequired: true);
        field.AddOption("Good", "good", 0, isDefault: true);

        db.Forms.Add(form);
        InvokePrepareForSave(db);

        db.Entry(form).Property(x => x.Id).IsTemporary.Should().BeTrue();
        db.Entry(field).Property(x => x.Id).IsTemporary.Should().BeTrue();
        field.FormId.Should().Be(form.Id);
        field.Options.Single().FormFieldId.Should().Be(field.Id);
        field.ValidationRules.Single().FormFieldId.Should().Be(field.Id);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void InvokePrepareForSave(ApplicationDbContext db)
    {
        typeof(ApplicationDbContext)
            .GetMethod("PrepareForSave", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(db, null);
    }
}
