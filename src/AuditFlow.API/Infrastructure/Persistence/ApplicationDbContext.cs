using System.Linq.Expressions;
using System.Reflection;

using AuditFlow.API.Domain.Common;
using AuditFlow.API.Domain.Common.Interfaces;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global turn off delete behaviour on foreign keys
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        ConfigureSoftDelete(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableRecords();
        UpdateSoftDeleteRecords();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableRecords()
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
    }

    private void UpdateSoftDeleteRecords()
    {
        var entries = ChangeTracker.Entries<ISoftDeletable>()
            .Where(p => p.State == EntityState.Deleted);

        foreach (EntityEntry<ISoftDeletable> softDeletable in entries)
        {
            softDeletable.State = EntityState.Modified;
            softDeletable.Entity.IsDeleted = true;
            softDeletable.Entity.DeletedAt = _dateTimeService.UtcNow;
        }
    }

    private static void ConfigureSoftDelete(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("IsDeleted")
                    .HasFilter("IsDeleted = 0");
            }
        }
    }

    /// <summary>
    /// Helper method to create a filter expression for `IsDeleted`
    /// </summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    private static LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, "IsDeleted");
        var constant = Expression.Constant(false);
        var body = Expression.Equal(property, constant);

        return Expression.Lambda(body, parameter);
    }
}
