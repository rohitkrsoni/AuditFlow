using AuditFlow.Consumer.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditFlow.Consumer.Persistence;

public interface IAuditDbContext
{
    DbSet<DataAuditTransaction> DataAuditTransactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
