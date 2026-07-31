namespace Vendo.FormBuilder.Domain.Entities;

/// <summary>
/// Read-only lookup used by <see cref="Enums.FieldType.Province"/> fields.
/// Reference data is seeded with the schema, so this entity has no audit or soft-delete state.
/// </summary>
public sealed class Province
{
    private Province()
    {
    }

    public Province(int id, string name, int orderIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        Id = id;
        Name = name.Trim();
        OrderIndex = orderIndex;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Display order for pickers. Lower values come first.</summary>
    public int OrderIndex { get; private set; }
}
