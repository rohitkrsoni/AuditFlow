using AuditFlow.Consumer.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditFlow.Consumer.Persistence;

public class AuditDbContext : DbContext, IAuditDbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<DataAuditTransaction> DataAuditTransactions => Set<DataAuditTransaction>();
}
