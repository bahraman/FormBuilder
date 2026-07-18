using FormBuilder.Domain.Entities;

namespace FormBuilder.Domain.Interfaces;

public interface IFormResponseRepository
{
    Task<FormResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<FormResponse> Items, int TotalCount)> GetByFormIdPagedAsync(
        Guid formId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(FormResponse response, CancellationToken cancellationToken = default);
}
