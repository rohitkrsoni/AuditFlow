using AuditFlow.Consumer.Entities;
using AuditFlow.Consumer.Persistence;
using AuditFlow.Shared.AuditContracts;
using FluentValidation;
using MassTransit;

namespace AuditFlow.Consumer.Consumers;

public sealed class AuditTransactionConsumer(IAuditDbContext dbContext, IValidator<AuditTransactionMessage> validator) : IConsumer<AuditTransactionMessage>
{
    public async Task Consume(ConsumeContext<AuditTransactionMessage> ctx)
    {
        var validationResult = await validator.ValidateAsync(ctx.Message, ctx.CancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var message = ctx.Message;

        var auditdetails = GetAuditDetails(message);
        var transaction = new DataAuditTransaction
        {
            EventId = message.EventId,
            IdentityUserId = message.IdentityUserId!,
            EventDateUtc = message.EventDateUtc,
            TransactionDetails = [.. auditdetails],
        };

        await dbContext.DataAuditTransactions.AddAsync(transaction, ctx.CancellationToken);

        await dbContext.SaveChangesAsync(ctx.CancellationToken);
    }

    public static IEnumerable<DataAuditTransactionDetail> GetAuditDetails(AuditTransactionMessage auditTransactionMessage)
    {

        foreach (var detailMessage in auditTransactionMessage.TransactionDetails!)
        {

            yield return new DataAuditTransactionDetail
            {
                DataAuditTransactionType = (DataAuditTransactionTypes)(int)detailMessage.DataAuditTransactionType!,
                EntityName = detailMessage.EntityName!,
                PrimaryKeyValue = detailMessage.PrimaryKeyValue!,
                PropertyName = detailMessage.PropertyName!,
                OriginalValue = detailMessage.OriginalValue,
                NewValue = detailMessage.NewValue
            };
        }
    }
}
