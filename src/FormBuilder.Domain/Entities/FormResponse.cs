using FormBuilder.Domain.Common;

namespace FormBuilder.Domain.Entities;

public class FormResponse : BaseEntity
{
    private readonly List<FormResponseValue> _values = [];

    public Guid FormId { get; private set; }
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
        Guid formId,
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

    public FormResponseValue AddValue(Guid fieldId, string fieldName, string? value)
    {
        var responseValue = FormResponseValue.Create(Id, fieldId, fieldName, value);
        _values.Add(responseValue);
        return responseValue;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedBy = deletedBy;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
