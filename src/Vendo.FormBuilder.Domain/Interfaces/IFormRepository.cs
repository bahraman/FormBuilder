using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Enums;

namespace Vendo.FormBuilder.Domain.Interfaces;

public interface IFormRepository
{
    Task<Form?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Form?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Form> Items, int TotalCount)> GetPagedAsync(
        int subscriberId,
        int? restaurantId,
        int pageNumber,
        int pageSize,
        string? search = null,
        FormStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(
        int subscriberId,
        int? restaurantId,
        string slug,
        Guid? excludeFormId = null,
        CancellationToken cancellationToken = default);
    Task<int> GetLatestVersionAsync(
        int subscriberId,
        int? restaurantId,
        string slug,
        CancellationToken cancellationToken = default);
    Task AddAsync(Form form, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a detached form as modified. No-op when the form is already tracked —
    /// do not use DbSet.Update on tracked aggregates (it marks new children as Modified
    /// and triggers false concurrency conflicts on rowversion tokens).
    /// </summary>
    void Update(Form form);

    /// <summary>
    /// Sets the original concurrency token so SaveChanges fails if the row changed.
    /// </summary>
    void SetOriginalRowVersion(Form form, byte[] rowVersion);

    /// <summary>
    /// Sets the original concurrency token on a field so SaveChanges fails if the row changed.
    /// </summary>
    void SetOriginalRowVersion(FormField field, byte[] rowVersion);
}
