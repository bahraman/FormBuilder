using Vendo.FormBuilder.Domain.Common;

namespace Vendo.FormBuilder.Domain.Entities;

public class FieldOption : BaseEntity
{
    public long FormFieldId { get; private set; }
    public FormField? FormField { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsDefault { get; private set; }

    private FieldOption()
    {
    }

    public static FieldOption Create(
        long formFieldId,
        string label,
        string value,
        int displayOrder,
        bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new FieldOption
        {
            FormFieldId = formFieldId,
            Label = label.Trim(),
            Value = value.Trim(),
            DisplayOrder = displayOrder,
            IsDefault = isDefault
        };
    }

    public void Update(string label, string value, int displayOrder, bool isDefault)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Label = label.Trim();
        Value = value.Trim();
        DisplayOrder = displayOrder;
        IsDefault = isDefault;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
