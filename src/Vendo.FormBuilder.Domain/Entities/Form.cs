using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Enums;
using Vendo.FormBuilder.Domain.Exceptions;

namespace Vendo.FormBuilder.Domain.Entities;

public class Form : LongEntity
{
    private readonly List<FormField> _fields = [];
    private readonly List<FormResponse> _responses = [];

    /// <summary>Required owner. Forms are always isolated by subscriber.</summary>
    public int SubscriberId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public FormStatus Status { get; private set; } = FormStatus.Draft;
    public int Version { get; private set; } = 1;
    public long? ParentFormId { get; private set; }
    public Form? ParentForm { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public DateTime? ArchivedAtUtc { get; private set; }

    public IReadOnlyCollection<FormField> Fields => _fields.AsReadOnly();
    public IReadOnlyCollection<FormResponse> Responses => _responses.AsReadOnly();

    private Form()
    {
    }

    public static Form Create(
        int subscriberId,
        string name,
        string? description,
        string slug,
        string? createdBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var tenant = TenantScope.ForSubscriber(subscriberId);

        return new Form
        {
            SubscriberId = tenant.SubscriberId,
            Name = name.Trim(),
            Description = description?.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Status = FormStatus.Draft,
            Version = 1,
            CreatedBy = createdBy
        };
    }

    public void EnsureAccessibleTo(TenantScope scope) =>
        scope.EnsureCanAccess(SubscriberId);

    public void Update(string name, string? description, string? updatedBy = null)
    {
        EnsureEditable();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Publish(string? updatedBy = null)
    {
        if (Status == FormStatus.Published)
        {
            throw new ConflictException("Form is already published.");
        }

        if (Status == FormStatus.Archived)
        {
            throw new ConflictException("Archived forms cannot be published. Create a new version instead.");
        }

        if (_fields.Count == 0 || _fields.All(f => f.IsDeleted))
        {
            throw new DomainException("A form must have at least one field before it can be published.");
        }

        Status = FormStatus.Published;
        PublishedAtUtc = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Archive(string? updatedBy = null)
    {
        if (Status == FormStatus.Archived)
        {
            throw new ConflictException("Form is already archived.");
        }

        if (Status != FormStatus.Published)
        {
            throw new ConflictException("Only published forms can be archived.");
        }

        Status = FormStatus.Archived;
        ArchivedAtUtc = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Form CreateNewVersion(string? createdBy = null)
    {
        if (Status != FormStatus.Published && Status != FormStatus.Archived)
        {
            throw new ConflictException("Only published or archived forms can be versioned.");
        }

        var newVersion = new Form
        {
            SubscriberId = SubscriberId,
            Name = Name,
            Description = Description,
            Slug = Slug,
            Status = FormStatus.Draft,
            Version = Version + 1,
            ParentFormId = Id,
            CreatedBy = createdBy
        };

        foreach (var field in _fields.Where(f => !f.IsDeleted).OrderBy(f => f.DisplayOrder))
        {
            // newVersion.Id is 0 until save; EF relationship fix-up assigns FKs on insert.
            newVersion.AddField(field.CloneForNewForm(newVersion.Id));
        }

        return newVersion;
    }

    public FormField AddField(
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
        EnsureEditable();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        if (_fields.Any(f => !f.IsDeleted && f.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A field with name '{name}' already exists on this form.");
        }

        var field = FormField.Create(
            Id,
            name,
            label,
            fieldType,
            displayOrder,
            isRequired,
            placeholder,
            helpText,
            defaultValue,
            createdBy);

        _fields.Add(field);
        // Do not mutate Form scalars here. Touching UpdatedAtUtc marks Form Modified and
        // triggers a rowversion UPDATE that frequently false-fails when adding children.
        return field;
    }

    internal void AddField(FormField field)
    {
        _fields.Add(field);
    }

    public void ReorderFields(IReadOnlyDictionary<long, int> fieldOrders, string? updatedBy = null)
    {
        EnsureEditable();

        foreach (var (fieldId, order) in fieldOrders)
        {
            var field = _fields.FirstOrDefault(f => f.Id == fieldId && !f.IsDeleted)
                ?? throw new NotFoundException(nameof(FormField), fieldId);

            field.SetDisplayOrder(order);
        }

        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public FormField GetField(long fieldId)
    {
        return _fields.FirstOrDefault(f => f.Id == fieldId && !f.IsDeleted)
            ?? throw new NotFoundException(nameof(FormField), fieldId);
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedBy = deletedBy;
        UpdatedAtUtc = DateTime.UtcNow;

        foreach (var field in _fields.Where(f => !f.IsDeleted))
        {
            field.SoftDelete(deletedBy);
        }
    }

    public void EnsureAcceptsSubmissions()
    {
        if (Status != FormStatus.Published)
        {
            throw new ConflictException("Responses can only be submitted to published forms.");
        }

        if (IsDeleted)
        {
            throw new ConflictException("Cannot submit responses to a deleted form.");
        }
    }

    private void EnsureEditable()
    {
        if (IsDeleted)
        {
            throw new ConflictException("Deleted forms cannot be modified.");
        }

        if (Status != FormStatus.Draft)
        {
            throw new ConflictException("Only draft forms can be modified. Create a new version to make changes.");
        }
    }
}
