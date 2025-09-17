namespace AuditFlow.Shared.AuditContracts;

public sealed record class AuditTransactionDetailMessage
{
    public DataAuditTransactionTypes DataAuditTransactionType { get; init; } = DataAuditTransactionTypes.Unknown;
    public string? EntityName { get; init; }
    public string? PrimaryKeyValue { get; init; }
    public string? PropertyName { get; init; }
    public string? OriginalValue { get; init; }
    public string? NewValue { get; init; }
}
