namespace AuditFlow.Shared.AuditContracts;

public sealed record class AuditTransactionMessage
{
    public Guid EventId { get; init; }
    public DateTimeOffset EventDateUtc { get; init; }
    public string? IdentityUserId { get; init; }
    public bool AuditSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public IReadOnlyList<AuditTransactionDetailMessage>? TransactionDetails { get; init; }
}
