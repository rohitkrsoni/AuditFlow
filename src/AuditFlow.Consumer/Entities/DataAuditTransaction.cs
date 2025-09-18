namespace AuditFlow.Consumer.Entities;

public class DataAuditTransaction
{
    public long Id { get; set; }

    public required string IdentityUserId { get; set; }

    public DateTimeOffset EventDateUtc { get; set; }

    public virtual ICollection<DataAuditTransactionDetail> TransactionDetails { get; set; }
}
