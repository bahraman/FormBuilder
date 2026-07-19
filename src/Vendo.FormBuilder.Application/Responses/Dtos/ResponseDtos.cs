namespace Vendo.FormBuilder.Application.Responses.Dtos;

public sealed class FormResponseDto
{
    public required Guid Id { get; init; }
    public required long FormId { get; init; }
    public string? SubmittedBy { get; init; }
    public required DateTime SubmittedAtUtc { get; init; }
    public required IReadOnlyList<FormResponseValueDto> Values { get; init; }
}

public sealed class FormResponseValueDto
{
    public required long FieldId { get; init; }
    public required string FieldName { get; init; }
    public string? Value { get; init; }
}

public sealed class FormResponseValueInputDto
{
    public required long FieldId { get; init; }
    public string? Value { get; init; }
}
