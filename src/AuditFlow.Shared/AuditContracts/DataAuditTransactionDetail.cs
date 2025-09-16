using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuditFlow.Shared.AuditContracts.Attributes;

namespace AuditFlow.Shared.AuditContracts;

[NonAuditable]
[Table("DataAuditTransactionDetails")]
public class DataAuditTransactionDetail
{
  public long Id { get; set; }

  public long DataAuditTransactionId { get; set; }
  public virtual DataAuditTransaction DataAuditTransaction { get; set; }

  public DataAuditTransactionTypes DataAuditTransactionType { get; set; }

  [Required]
  [MaxLength(255)]
  public required string EntityName { get; set; }

  [Required]
  public required string PrimaryKeyValue { get; set; }

  [Required]
  [MaxLength(255)]
  public required string PropertyName { get; set; }
  public required string? OriginalValue { get; set; }
  public required string? NewValue { get; set; }
}
