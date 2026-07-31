using Vendo.FormBuilder.Domain.Enums;

namespace Vendo.FormBuilder.Application.Forms.Dtos;

public class FormSummaryDto
{
    public required long Id { get; init; }
    public required int SubscriberId { get; init; }
    public int? RestaurantId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Slug { get; init; }
    public required FormStatus Status { get; init; }
    public required int Version { get; init; }
    public long? ParentFormId { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public DateTime? ArchivedAtUtc { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public required int FieldCount { get; init; }
    public required string RowVersion { get; init; }
}

public sealed class FormDetailDto : FormSummaryDto
{
    public required IReadOnlyList<FormFieldDto> Fields { get; init; }
}

public sealed class FormFieldDto
{
    public required long Id { get; init; }
    public required long FormId { get; init; }
    public required string Name { get; init; }
    public required string Label { get; init; }
    public required FieldType FieldType { get; init; }
    public required int DisplayOrder { get; init; }
    public required bool IsRequired { get; init; }
    public string? Placeholder { get; init; }
    public string? HelpText { get; init; }
    public string? DefaultValue { get; init; }
    public required IReadOnlyList<FieldOptionDto> Options { get; init; }
    public required IReadOnlyList<FieldValidationRuleDto> ValidationRules { get; init; }
    public required string RowVersion { get; init; }
}

public sealed class FieldOptionDto
{
    public required Guid Id { get; init; }
    public required string Label { get; init; }
    public required string Value { get; init; }
    public required int DisplayOrder { get; init; }
    public required bool IsDefault { get; init; }
}

public sealed class FieldValidationRuleDto
{
    public required Guid Id { get; init; }
    public required ValidationRuleType RuleType { get; init; }
    public required string Value { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class FieldOptionInputDto
{
    public required string Label { get; init; }
    public required string Value { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsDefault { get; init; }
}

public sealed class FieldValidationRuleInputDto
{
    public required ValidationRuleType RuleType { get; init; }
    public required string Value { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class FieldOrderItemDto
{
    public required long FieldId { get; init; }
    public required int DisplayOrder { get; init; }
}
