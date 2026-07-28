namespace Vendo.FormBuilder.Domain.Entities;

/// <summary>
/// Read-only lookup used by <see cref="Enums.FieldType.City"/> fields.
/// Every city belongs to exactly one <see cref="Entities.Province"/>.
/// </summary>
public sealed class City
{
    private City()
    {
    }

    public City(int id, int provinceId, string name, int orderIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(provinceId);

        Id = id;
        ProvinceId = provinceId;
        Name = name.Trim();
        OrderIndex = orderIndex;
    }

    public int Id { get; private set; }
    public int ProvinceId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Display order within the province. Lower values come first.</summary>
    public int OrderIndex { get; private set; }

    public Province? Province { get; private set; }
}
