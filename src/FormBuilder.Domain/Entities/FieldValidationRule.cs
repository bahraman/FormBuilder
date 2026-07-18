using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;

namespace FormBuilder.Domain.Entities;

public class FieldValidationRule : BaseEntity
{
    public Guid FormFieldId { get; private set; }
    public FormField? FormField { get; private set; }
    public ValidationRuleType RuleType { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    private FieldValidationRule()
    {
    }

    public static FieldValidationRule Create(
        Guid formFieldId,
        ValidationRuleType ruleType,
        string value,
        string? errorMessage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new FieldValidationRule
        {
            FormFieldId = formFieldId,
            RuleType = ruleType,
            Value = value.Trim(),
            ErrorMessage = errorMessage?.Trim()
        };
    }

    public void Update(string value, string? errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value.Trim();
        ErrorMessage = errorMessage?.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
