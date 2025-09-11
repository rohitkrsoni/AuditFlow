using AuditFlow.API.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace AuditFlow.API.Infrastructure.Persistence;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
