using System.Runtime.CompilerServices;
using AuditFlow.API.Domain.Common.Interfaces;
using AuditFlow.API.Infrastructure.Services;
using AuditFlow.Shared.AuditContracts;
using AuditFlow.Shared.AuditContracts.Attributes;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuditFlow.API.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ConditionalWeakTable<DbContext, List<AuditTransactionDetailMessage>> _state = [];
    private readonly IPublishEndpoint _endpointPublisher;

    public AuditInterceptor(
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IPublishEndpoint endpointPublisher)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _endpointPublisher = endpointPublisher;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var auditableEntries = GetAuditableEntries(eventData.Context.ChangeTracker);

        if (auditableEntries.Count == 0)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var dataAuditTransactionDetails = auditableEntries
            .SelectMany(g => g)
            .SelectMany(e => CreateAuditDetails(e, e.State))
            .ToList();

        if (dataAuditTransactionDetails.Count > 0)
        {
            var list = _state.GetValue(eventData.Context, _ => []);
            list.AddRange(dataAuditTransactionDetails);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx is not null && _state.TryGetValue(ctx, out var details) && details.Count > 0)
        {
            var transaction = new AuditTransactionMessage {
                EventId = Guid.CreateVersion7(),
                EventDateUtc = _dateTimeService.UtcNow,
                IdentityUserId = _currentUserService.IdentityId,
                TransactionDetails = details,
                AuditSuccess = true
            };

            await _endpointPublisher.Publish(transaction, cancellationToken);
            _state.Remove(ctx);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);

    }

    public IEnumerable<AuditTransactionDetailMessage> CreateAuditDetails(EntityEntry dbEntry, EntityState state)
    {
        string entityName = dbEntry.Entity.GetType().Name;
        var primaryKeyName = dbEntry.Metadata.FindPrimaryKey()?.Properties[0].Name;
        var primaryKeyValue = primaryKeyName != null ? dbEntry.CurrentValues[primaryKeyName]?.ToString() : null;

        foreach (var propertyName in dbEntry.OriginalValues.Properties.Select(property => property.Name))
        {
            bool propertyCanBeAudited = Attribute.GetCustomAttribute(
                dbEntry.Entity.GetType().GetProperty(propertyName)!, typeof(NonAuditableAttribute)) == null;

            (object? oldValue, object? newValue) = GetOldAndNewValues(dbEntry, propertyName, state);

            if (state != EntityState.Modified || !Equals(oldValue, newValue))
            {
                yield return new AuditTransactionDetailMessage
                {
                    DataAuditTransactionType = IsSoftDeleteEvent(dbEntry) ?
                        DataAuditTransactionTypes.SoftDelete : ToAuditType(state),
                    EntityName = entityName,
                    PrimaryKeyValue = primaryKeyValue,
                    PropertyName = propertyName,
                    OriginalValue = propertyCanBeAudited ? GetValueString(oldValue) : "XXX",
                    NewValue = propertyCanBeAudited ? GetValueString(newValue) : "XXX",
                };
            }
        }
    }

    public static DataAuditTransactionTypes ToAuditType(EntityState state)
    {
        return state switch
        {
            EntityState.Added => DataAuditTransactionTypes.Insert,
            EntityState.Modified => DataAuditTransactionTypes.Update,
            EntityState.Deleted => DataAuditTransactionTypes.Delete,
            _ => DataAuditTransactionTypes.Unknown
        };
    }

    private static Tuple<object?, object?> GetOldAndNewValues(EntityEntry dbEntry, string propertyName, EntityState state)
    {
        return state switch
        {
            EntityState.Added => new Tuple<object?, object?>(null, dbEntry.CurrentValues[propertyName]),
            EntityState.Modified => new Tuple<object?, object?>(dbEntry.OriginalValues[propertyName], dbEntry.CurrentValues[propertyName]),
            EntityState.Deleted => new Tuple<object?, object?>(dbEntry.OriginalValues[propertyName], null),
            _ => new Tuple<object?, object?>(null, null),
        };
    }

    private static string? GetValueString(object? item)
    {
        if (item == null)
        {
            return null;
        }

        return item is not PropertyValues dbItem ? item.ToString() : dbItem.ToObject().ToString();
    }
  

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;

        if (ctx == null)
        {
            return;
        }

        var transaction = new AuditTransactionMessage
        {
            EventId = Guid.CreateVersion7(),
            EventDateUtc = _dateTimeService.UtcNow,
            IdentityUserId = _currentUserService.IdentityId,
            TransactionDetails = [],
            AuditSuccess = false,
            ErrorMessage = eventData.Exception.Message
        };

        await _endpointPublisher.Publish(transaction, cancellationToken);

        _state.Remove(ctx);

    }

    private static ILookup<EntityState, EntityEntry> GetAuditableEntries(ChangeTracker changeTracker)
    {
        IEnumerable<EntityEntry> auditableEntries;
        try
        {
            auditableEntries = changeTracker.Entries<IAuditableEntity>()
                .Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);
        }
        catch (Exception)
        {
            auditableEntries = [];
        }
        return auditableEntries.ToLookup(e => e.State);
    }
    private static bool IsSoftDeleteEvent(EntityEntry entry)
    {
        return entry.State == EntityState.Modified
            && HasProperty(entry, "IsDeleted")
            && Equals(entry.OriginalValues["IsDeleted"], false)
            && Equals(entry.CurrentValues["IsDeleted"], true);
    }

    private static bool HasProperty(EntityEntry entry, string name)
    {
        return entry.Metadata.FindProperty(name) is not null;
    }

}

