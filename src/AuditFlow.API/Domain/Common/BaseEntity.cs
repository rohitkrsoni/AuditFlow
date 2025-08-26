
using AuditFlow.API.Domain.Common.Interfaces;

namespace AuditFlow.API.Domain.Common;

public class BaseEntity : ISoftDeletable, IAuditableEntity
{
  public bool IsDeleted { get; set; }
  public DateTimeOffset? DeletedAt { get; set; }
  public string CreatedBy { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public string UpdatedBy { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}
