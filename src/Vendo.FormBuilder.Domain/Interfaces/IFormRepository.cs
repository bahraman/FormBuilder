using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Enums;

namespace Vendo.FormBuilder.Domain.Interfaces;

public interface IFormRepository
{
    Task<Form?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Form?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Form> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        FormStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeFormId = null, CancellationToken cancellationToken = default);
    Task<int> GetLatestVersionAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(Form form, CancellationToken cancellationToken = default);
    void Update(Form form);
}
