using Vendo.FormBuilder.Domain.Common;

namespace Vendo.FormBuilder.Domain.Entities;

public class FormResponseValue : BaseEntity
{
    public Guid FormResponseId { get; private set; }
    public FormResponse? FormResponse { get; private set; }
    public long FormFieldId { get; private set; }
    public FormField? FormField { get; private set; }
    public string FieldName { get; private set; } = string.Empty;
    public string? Value { get; private set; }

    private FormResponseValue()
    {
    }

    public static FormResponseValue Create(
        Guid formResponseId,
        long formFieldId,
        string fieldName,
        string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        return new FormResponseValue
        {
            FormResponseId = formResponseId,
            FormFieldId = formFieldId,
            FieldName = fieldName.Trim(),
            Value = value
        };
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedBy = deletedBy;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
