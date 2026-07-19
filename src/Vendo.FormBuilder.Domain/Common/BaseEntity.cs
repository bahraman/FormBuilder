namespace Vendo.FormBuilder.Domain.Common;

public abstract class AuditableEntity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}

public abstract class BaseEntity<TId> : AuditableEntity
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; protected set; }
}

/// <summary>Default entity base using client-generated Guid keys.</summary>
public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}

/// <summary>Entity base using SQL Server IDENTITY (bigint) keys.</summary>
public abstract class LongEntity : BaseEntity<long>
{
    private static long _temporaryId;

    protected LongEntity()
    {
        // Distinct temporary keys for in-memory graphs; replaced by IDENTITY on insert.
        Id = Interlocked.Decrement(ref _temporaryId);
    }
}
