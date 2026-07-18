using System.Globalization;
using System.Text.RegularExpressions;
using Vendo.FormBuilder.Application.Common.Exceptions;
using Vendo.FormBuilder.Application.Responses.Dtos;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Enums;

namespace Vendo.FormBuilder.Application.Responses.Services;

public static class FormResponseValidator
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new(
        @"^\+?[0-9\s\-().]{7,20}$",
        RegexOptions.Compiled);

    private static readonly Regex UrlRegex = new(
        @"^https?:\/\/[^\s/$.?#].[^\s]*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static void Validate(Form form, IReadOnlyList<FormResponseValueInputDto> values)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        var fields = form.Fields.Where(f => !f.IsDeleted).ToDictionary(f => f.Id);
        var valueMap = values
            .GroupBy(v => v.FieldId)
            .ToDictionary(g => g.Key, g => g.Last().Value);

        foreach (var unknownFieldId in valueMap.Keys.Where(id => !fields.ContainsKey(id)))
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(
                unknownFieldId.ToString(),
                $"Field '{unknownFieldId}' does not belong to this form."));
        }

        foreach (var field in fields.Values)
        {
            valueMap.TryGetValue(field.Id, out var rawValue);
            ValidateField(field, rawValue, failures);
        }

        if (failures.Count > 0)
        {
            throw new ApplicationValidationException(failures);
        }
    }

    private static void ValidateField(
        FormField field,
        string? rawValue,
        List<FluentValidation.Results.ValidationFailure> failures)
    {
        var propertyName = field.Name;
        var isEmpty = string.IsNullOrWhiteSpace(rawValue);

        if (field.IsRequired && isEmpty)
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(
                propertyName,
                GetRuleMessage(field, ValidationRuleType.Required) ?? $"{field.Label} is required."));
            return;
        }

        if (isEmpty)
        {
            return;
        }

        switch (field.FieldType)
        {
            case FieldType.Number:
                if (!long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    failures.Add(Failure(propertyName, $"{field.Label} must be a valid integer."));
                }
                break;
            case FieldType.Decimal:
                if (!decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                {
                    failures.Add(Failure(propertyName, $"{field.Label} must be a valid decimal number."));
                }
                break;
            case FieldType.Date:
                if (!DateOnly.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    failures.Add(Failure(propertyName, $"{field.Label} must be a valid date (yyyy-MM-dd)."));
                }
                break;
            case FieldType.Time:
                if (!TimeOnly.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    failures.Add(Failure(propertyName, $"{field.Label} must be a valid time (HH:mm[:ss])."));
                }
                break;
            case FieldType.DateTime:
                if (!DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                {
                    failures.Add(Failure(propertyName, $"{field.Label} must be a valid date-time."));
                }
                break;
            case FieldType.Email:
                if (!EmailRegex.IsMatch(rawValue!))
                {
                    failures.Add(Failure(propertyName, GetRuleMessage(field, ValidationRuleType.Email) ?? $"{field.Label} must be a valid email address."));
                }
                break;
            case FieldType.Phone:
                if (!PhoneRegex.IsMatch(rawValue!))
                {
                    failures.Add(Failure(propertyName, GetRuleMessage(field, ValidationRuleType.Phone) ?? $"{field.Label} must be a valid phone number."));
                }
                break;
            case FieldType.Url:
                if (!UrlRegex.IsMatch(rawValue!))
                {
                    failures.Add(Failure(propertyName, GetRuleMessage(field, ValidationRuleType.Url) ?? $"{field.Label} must be a valid URL."));
                }
                break;
            case FieldType.RadioButton:
            case FieldType.Dropdown:
                EnsureOptionSelected(field, rawValue!, failures);
                break;
            case FieldType.MultiSelect:
            case FieldType.Checkbox:
                EnsureOptionsSelected(field, rawValue!, failures);
                break;
        }

        foreach (var rule in field.ValidationRules.Where(r => !r.IsDeleted))
        {
            ApplyRule(field, rule, rawValue!, failures);
        }
    }

    private static void ApplyRule(
        FormField field,
        FieldValidationRule rule,
        string rawValue,
        List<FluentValidation.Results.ValidationFailure> failures)
    {
        switch (rule.RuleType)
        {
            case ValidationRuleType.MinLength when int.TryParse(rule.Value, out var minLength) && rawValue.Length < minLength:
                failures.Add(Failure(field.Name, rule.ErrorMessage ?? $"{field.Label} must be at least {minLength} characters."));
                break;
            case ValidationRuleType.MaxLength when int.TryParse(rule.Value, out var maxLength) && rawValue.Length > maxLength:
                failures.Add(Failure(field.Name, rule.ErrorMessage ?? $"{field.Label} must be at most {maxLength} characters."));
                break;
            case ValidationRuleType.MinValue when decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var minActual)
                                                  && decimal.TryParse(rule.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var min)
                                                  && minActual < min:
                failures.Add(Failure(field.Name, rule.ErrorMessage ?? $"{field.Label} must be at least {min}."));
                break;
            case ValidationRuleType.MaxValue when decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var maxActual)
                                                  && decimal.TryParse(rule.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var max)
                                                  && maxActual > max:
                failures.Add(Failure(field.Name, rule.ErrorMessage ?? $"{field.Label} must be at most {max}."));
                break;
            case ValidationRuleType.Regex:
                try
                {
                    if (!Regex.IsMatch(rawValue, rule.Value))
                    {
                        failures.Add(Failure(field.Name, rule.ErrorMessage ?? $"{field.Label} is invalid."));
                    }
                }
                catch (ArgumentException)
                {
                    failures.Add(Failure(field.Name, $"Validation rule for {field.Label} has an invalid regex pattern."));
                }
                break;
        }
    }

    private static void EnsureOptionSelected(
        FormField field,
        string rawValue,
        List<FluentValidation.Results.ValidationFailure> failures)
    {
        var allowed = field.Options.Where(o => !o.IsDeleted).Select(o => o.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!allowed.Contains(rawValue))
        {
            failures.Add(Failure(field.Name, $"{field.Label} has an invalid option."));
        }
    }

    private static void EnsureOptionsSelected(
        FormField field,
        string rawValue,
        List<FluentValidation.Results.ValidationFailure> failures)
    {
        var selected = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var allowed = field.Options.Where(o => !o.IsDeleted).Select(o => o.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (selected.Length == 0)
        {
            if (field.IsRequired)
            {
                failures.Add(Failure(field.Name, $"{field.Label} is required."));
            }

            return;
        }

        foreach (var value in selected.Where(v => !allowed.Contains(v)))
        {
            failures.Add(Failure(field.Name, $"{field.Label} contains invalid option '{value}'."));
        }
    }

    private static string? GetRuleMessage(FormField field, ValidationRuleType ruleType) =>
        field.ValidationRules.FirstOrDefault(r => !r.IsDeleted && r.RuleType == ruleType)?.ErrorMessage;

    private static FluentValidation.Results.ValidationFailure Failure(string propertyName, string message) =>
        new(propertyName, message);
}
