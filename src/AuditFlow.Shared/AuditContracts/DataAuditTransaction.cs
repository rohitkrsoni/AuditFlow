using AuditFlow.Shared.AuditContracts.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace AuditFlow.Shared.AuditContracts;

[NonAuditable]
[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
public class DataAuditTransaction
{
  public long Id { get; set; }

  public required string IdentityUserId { get; set; }

  public DateTimeOffset EventDateUtc { get; set; }

  public virtual required ICollection<DataAuditTransactionDetail> TransactionDetails { get; set; }

  public bool AuditSuccess { get; set; } = true;

  public string ErrorMessage { get; set; } = string.Empty;
}
