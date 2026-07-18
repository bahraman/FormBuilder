using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;
using FormBuilder.Domain.Exceptions;

namespace FormBuilder.Domain.Entities;

public class FormField : BaseEntity
{
    private readonly List<FieldOption> _options = [];
    private readonly List<FieldValidationRule> _validationRules = [];

    public Guid FormId { get; private set; }
    public Form? Form { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public FieldType FieldType { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsRequired { get; private set; }
    public string? Placeholder { get; private set; }
    public string? HelpText { get; private set; }
    public string? DefaultValue { get; private set; }

    public IReadOnlyCollection<FieldOption> Options => _options.AsReadOnly();
    public IReadOnlyCollection<FieldValidationRule> ValidationRules => _validationRules.AsReadOnly();

    private FormField()
    {
    }

    public static FormField Create(
        Guid formId,
        string name,
        string label,
        FieldType fieldType,
        int displayOrder,
        bool isRequired = false,
        string? placeholder = null,
        string? helpText = null,
        string? defaultValue = null,
        string? createdBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        var field = new FormField
        {
            FormId = formId,
            Name = name.Trim(),
            Label = label.Trim(),
            FieldType = fieldType,
            DisplayOrder = displayOrder,
            IsRequired = isRequired,
            Placeholder = placeholder?.Trim(),
            HelpText = helpText?.Trim(),
            DefaultValue = defaultValue,
            CreatedBy = createdBy
        };

        if (isRequired)
        {
            field.AddValidationRule(ValidationRuleType.Required, "true", "This field is required.");
        }

        return field;
    }

    public void Update(
        string label,
        bool isRequired,
        string? placeholder,
        string? helpText,
        string? defaultValue,
        string? updatedBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Label = label.Trim();
        IsRequired = isRequired;
        Placeholder = placeholder?.Trim();
        HelpText = helpText?.Trim();
        DefaultValue = defaultValue;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = DateTime.UtcNow;

        SyncRequiredRule(isRequired);
    }

    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order cannot be negative.");
        }

        DisplayOrder = displayOrder;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public FieldOption AddOption(string label, string value, int displayOrder, bool isDefault = false)
    {
        EnsureSupportsOptions();
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (_options.Any(o => !o.IsDeleted && o.Value.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"An option with value '{value}' already exists.");
        }

        var option = FieldOption.Create(Id, label, value, displayOrder, isDefault);
        _options.Add(option);
        UpdatedAtUtc = DateTime.UtcNow;
        return option;
    }

    public void ReplaceOptions(IEnumerable<(string Label, string Value, int DisplayOrder, bool IsDefault)> options)
    {
        EnsureSupportsOptions();

        foreach (var option in _options.Where(o => !o.IsDeleted))
        {
            option.SoftDelete();
        }

        foreach (var (label, value, displayOrder, isDefault) in options)
        {
            AddOption(label, value, displayOrder, isDefault);
        }
    }

    public FieldValidationRule AddValidationRule(ValidationRuleType ruleType, string value, string? errorMessage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var existing = _validationRules.FirstOrDefault(r => !r.IsDeleted && r.RuleType == ruleType);
        if (existing is not null)
        {
            existing.Update(value, errorMessage);
            UpdatedAtUtc = DateTime.UtcNow;
            return existing;
        }

        var rule = FieldValidationRule.Create(Id, ruleType, value, errorMessage);
        _validationRules.Add(rule);
        UpdatedAtUtc = DateTime.UtcNow;
        return rule;
    }

    public void ReplaceValidationRules(IEnumerable<(ValidationRuleType RuleType, string Value, string? ErrorMessage)> rules)
    {
        foreach (var rule in _validationRules.Where(r => !r.IsDeleted && r.RuleType != ValidationRuleType.Required))
        {
            rule.SoftDelete();
        }

        foreach (var (ruleType, value, errorMessage) in rules.Where(r => r.RuleType != ValidationRuleType.Required))
        {
            AddValidationRule(ruleType, value, errorMessage);
        }

        SyncRequiredRule(IsRequired);
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedBy = deletedBy;
        UpdatedAtUtc = DateTime.UtcNow;

        foreach (var option in _options.Where(o => !o.IsDeleted))
        {
            option.SoftDelete();
        }

        foreach (var rule in _validationRules.Where(r => !r.IsDeleted))
        {
            rule.SoftDelete();
        }
    }

    public FormField CloneForNewForm(Guid newFormId)
    {
        var clone = Create(
            newFormId,
            Name,
            Label,
            FieldType,
            DisplayOrder,
            IsRequired,
            Placeholder,
            HelpText,
            DefaultValue);

        foreach (var option in _options.Where(o => !o.IsDeleted).OrderBy(o => o.DisplayOrder))
        {
            clone.AddOption(option.Label, option.Value, option.DisplayOrder, option.IsDefault);
        }

        foreach (var rule in _validationRules.Where(r => !r.IsDeleted && r.RuleType != ValidationRuleType.Required))
        {
            clone.AddValidationRule(rule.RuleType, rule.Value, rule.ErrorMessage);
        }

        return clone;
    }

    public bool SupportsOptions() =>
        FieldType is FieldType.RadioButton or FieldType.Dropdown or FieldType.MultiSelect or FieldType.Checkbox;

    private void EnsureSupportsOptions()
    {
        if (!SupportsOptions())
        {
            throw new DomainException($"Field type '{FieldType}' does not support selectable options.");
        }
    }

    private void SyncRequiredRule(bool isRequired)
    {
        var requiredRule = _validationRules.FirstOrDefault(r => !r.IsDeleted && r.RuleType == ValidationRuleType.Required);

        if (isRequired)
        {
            if (requiredRule is null)
            {
                AddValidationRule(ValidationRuleType.Required, "true", "This field is required.");
            }
        }
        else if (requiredRule is not null)
        {
            requiredRule.SoftDelete();
        }
    }
}
