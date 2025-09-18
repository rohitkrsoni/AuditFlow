using AuditFlow.Shared.AuditContracts;
using MassTransit;
using AuditFlow.Consumer.Persistence;
using AuditFlow.Consumer.Entities;

namespace AuditFlow.Consumer.Consumers;

public sealed class AuditTransactionConsumer(IAuditDbContext dbContext) : IConsumer<AuditTransactionMessage>
{
    public async Task Consume(ConsumeContext<AuditTransactionMessage> ctx)
    {
        var message = ctx.Message;

        var auditdetails = GetAuditDetails(message);
        var transaction = new DataAuditTransaction
        {
            IdentityUserId = message.IdentityUserId!,
            EventDateUtc = message.EventDateUtc,
            TransactionDetails = [.. auditdetails],
        };

        await dbContext.DataAuditTransactions.AddAsync(transaction, ctx.CancellationToken);

        await dbContext.SaveChangesAsync(ctx.CancellationToken);
    }

    public static IEnumerable<DataAuditTransactionDetail> GetAuditDetails(AuditTransactionMessage auditTransactionMessage)
    {
        if (auditTransactionMessage.TransactionDetails is null)
        {
            yield break;
        }

        foreach (var detailMessage in auditTransactionMessage.TransactionDetails)
        {
            if (detailMessage.EntityName is null || detailMessage.PrimaryKeyValue is null || detailMessage.PropertyName is null)
            {
                continue;
            }

            yield return new DataAuditTransactionDetail
            {
                DataAuditTransactionType = (DataAuditTransactionTypes)(int)detailMessage.DataAuditTransactionType,
                EntityName = detailMessage.EntityName!,
                PrimaryKeyValue = detailMessage.PrimaryKeyValue!,
                PropertyName = detailMessage.PropertyName!,
                OriginalValue = detailMessage.OriginalValue,
                NewValue = detailMessage.NewValue
            };
        }
    }
}
