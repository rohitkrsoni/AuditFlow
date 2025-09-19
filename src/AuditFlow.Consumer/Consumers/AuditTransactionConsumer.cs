using AuditFlow.Consumer.Entities;
using AuditFlow.Consumer.Persistence;
using AuditFlow.Shared.AuditContracts;
using FluentValidation;
using MassTransit;

namespace AuditFlow.Consumer.Consumers;

public sealed class AuditTransactionConsumer(
    IAuditDbContext dbContext,
    IValidator<AuditTransactionMessage> validator,
    ILogger<AuditTransactionConsumer> logger
    ) : IConsumer<AuditTransactionMessage>
{
    public async Task Consume(ConsumeContext<AuditTransactionMessage> ctx)
    {
        logger.LogInformation("Received AuditTransactionMessage with EventId {EventId}", ctx.Message.EventId);

        logger.LogInformation("Validating AuditTransactionMessage with EventId {EventId}", ctx.Message.EventId);

        var validationResult = await validator.ValidateAsync(ctx.Message, ctx.CancellationToken);

        if (!validationResult.IsValid)
        {
            logger.LogWarning("Validation failed for AuditTransactionMessage with EventId {EventId}: {Errors}", ctx.Message.EventId, validationResult.Errors);
            logger.LogWarning("Moving message to error queue.");
            throw new ValidationException(validationResult.Errors);
        }

        logger.LogInformation("Validation succeeded for AuditTransactionMessage with EventId {EventId}", ctx.Message.EventId);

        var message = ctx.Message;

        var auditdetails = GetAuditDetails(message);
        var transaction = new DataAuditTransaction
        {
            EventId = message.EventId,
            IdentityUserId = message.IdentityUserId!,
            EventDateUtc = message.EventDateUtc,
            TransactionDetails = [.. auditdetails],
        };
        logger.LogInformation("Storing AuditTransactionMessage with EventId {EventId} to database", ctx.Message.EventId);

        await dbContext.DataAuditTransactions.AddAsync(transaction, ctx.CancellationToken);

        await dbContext.SaveChangesAsync(ctx.CancellationToken);

        logger.LogInformation("Successfully stored AuditTransactionMessage with EventId {EventId} to database", ctx.Message.EventId);
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
