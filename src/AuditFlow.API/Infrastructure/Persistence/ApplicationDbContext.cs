using System.Reflection;

using AuditFlow.API.Domain.Common.Interfaces;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;

namespace AuditFlow.API.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
  private readonly ICurrentUserService _currentUserService;
  private readonly IDateTimeService _dateTimeService;

  public ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService,
    IDateTimeService dateTimeService) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public DbSet<Product> Products => Set<Product>();

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
    {
      switch (entry.State)
      {
        case EntityState.Added:
          entry.Entity.CreatedBy = _currentUserService.IdentityId;
          entry.Entity.CreatedAt = _dateTimeService.UtcNow;
          entry.Entity.UpdatedBy = _currentUserService.IdentityId;
          entry.Entity.UpdatedAt = _dateTimeService.UtcNow;
          break;

        case EntityState.Modified:
          entry.Entity.UpdatedBy = _currentUserService.IdentityId;
          entry.Entity.UpdatedAt = _dateTimeService.UtcNow;
          break;

        default:
          break;
      }
    }
    return await base.SaveChangesAsync(cancellationToken);
  }
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    base.OnModelCreating(modelBuilder);
  }
}
