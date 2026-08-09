using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Application.Responses.Dtos;
using Vendo.FormBuilder.Domain.Entities;

namespace Vendo.FormBuilder.Application.Common.Mappings;

public static class FormMappings
{
    public static FormSummaryDto ToSummaryDto(this Form form) => new()
    {
        Id = form.Id,
        SubscriberId = form.SubscriberId,
        Name = form.Name,
        Description = form.Description,
        Slug = form.Slug,
        Status = form.Status,
        Version = form.Version,
        ParentFormId = form.ParentFormId,
        PublishedAtUtc = form.PublishedAtUtc,
        ArchivedAtUtc = form.ArchivedAtUtc,
        CreatedAtUtc = form.CreatedAtUtc,
        UpdatedAtUtc = form.UpdatedAtUtc,
        FieldCount = form.Fields.Count(f => !f.IsDeleted),
        RowVersion = Convert.ToBase64String(form.RowVersion)
    };

    public static FormDetailDto ToDetailDto(this Form form) => new()
    {
        Id = form.Id,
        SubscriberId = form.SubscriberId,
        Name = form.Name,
        Description = form.Description,
        Slug = form.Slug,
        Status = form.Status,
        Version = form.Version,
        ParentFormId = form.ParentFormId,
        PublishedAtUtc = form.PublishedAtUtc,
        ArchivedAtUtc = form.ArchivedAtUtc,
        CreatedAtUtc = form.CreatedAtUtc,
        UpdatedAtUtc = form.UpdatedAtUtc,
        FieldCount = form.Fields.Count(f => !f.IsDeleted),
        RowVersion = Convert.ToBase64String(form.RowVersion),
        Fields = form.Fields
            .Where(f => !f.IsDeleted)
            .OrderBy(f => f.DisplayOrder)
            .Select(f => f.ToDto())
            .ToList()
    };

    public static FormFieldDto ToDto(this FormField field) => new()
    {
        Id = field.Id,
        FormId = field.FormId,
        Name = field.Name,
        Label = field.Label,
        FieldType = field.FieldType,
        DisplayOrder = field.DisplayOrder,
        IsRequired = field.IsRequired,
        Placeholder = field.Placeholder,
        HelpText = field.HelpText,
        DefaultValue = field.DefaultValue,
        Options = field.Options
            .Where(o => !o.IsDeleted)
            .OrderBy(o => o.DisplayOrder)
            .Select(o => o.ToDto())
            .ToList(),
        ValidationRules = field.ValidationRules
            .Where(r => !r.IsDeleted)
            .Select(r => r.ToDto())
            .ToList(),
        RowVersion = Convert.ToBase64String(field.RowVersion)
    };

    public static FieldOptionDto ToDto(this FieldOption option) => new()
    {
        Id = option.Id,
        Label = option.Label,
        Value = option.Value,
        DisplayOrder = option.DisplayOrder,
        IsDefault = option.IsDefault
    };

    public static FieldValidationRuleDto ToDto(this FieldValidationRule rule) => new()
    {
        Id = rule.Id,
        RuleType = rule.RuleType,
        Value = rule.Value,
        ErrorMessage = rule.ErrorMessage
    };

    public static FormResponseDto ToDto(this FormResponse response) => new()
    {
        Id = response.Id,
        FormId = response.FormId,
        SubmittedBy = response.SubmittedBy,
        SubmittedAtUtc = response.SubmittedAtUtc,
        Values = response.Values
            .Where(v => !v.IsDeleted)
            .Select(v => v.ToDto())
            .ToList()
    };

    public static FormResponseValueDto ToDto(this FormResponseValue value) => new()
    {
        FieldId = value.FormFieldId,
        FieldName = value.FieldName,
        Value = value.Value
    };
}
