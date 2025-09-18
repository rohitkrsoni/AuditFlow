using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditFlow.Consumer.Entities;

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
    public string? OriginalValue { get; set; }
    public string? NewValue { get; set; }
}
