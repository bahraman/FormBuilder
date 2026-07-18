using FormBuilder.Domain.Exceptions;
using FormBuilder.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("The resource was modified by another request. Reload and try again.");
        }
    }
}
