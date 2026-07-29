using Vendo.FormBuilder.Domain.Common;

namespace Vendo.FormBuilder.Domain.Entities;

public class FormResponse : BaseEntity
{
    private readonly List<FormResponseValue> _values = [];

    public long FormId { get; private set; }
    public Form? Form { get; private set; }
    public string? SubmittedBy { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public IReadOnlyCollection<FormResponseValue> Values => _values.AsReadOnly();

    private FormResponse()
    {
    }

    public static FormResponse Create(
        long formId,
        string? submittedBy = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new FormResponse
        {
            FormId = formId,
            SubmittedBy = submittedBy,
            SubmittedAtUtc = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedBy = submittedBy
        };
    }

    public FormResponseValue AddValue(long fieldId, string fieldName, string? value)
    {
        var responseValue = FormResponseValue.Create(Id, fieldId, fieldName, value);
        _values.Add(responseValue);
        return responseValue;
    }

    public void ReplaceValues(
        IEnumerable<(long FieldId, string FieldName, string? Value)> values,
        string? updatedBy = null)
    {
        foreach (var existing in _values.Where(v => !v.IsDeleted))
        {
            existing.SoftDelete(updatedBy);
        }

        foreach (var value in values)
        {
            AddValue(value.FieldId, value.FieldName, value.Value);
        }

        UpdatedBy = updatedBy;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedBy = deletedBy;
        UpdatedAtUtc = DateTime.UtcNow;

        foreach (var value in _values.Where(v => !v.IsDeleted))
        {
            value.SoftDelete(deletedBy);
        }
    }
}
