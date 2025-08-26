namespace AuditFlow.API.Domain.Common.Interfaces;

public interface ISoftDeletable
{
  bool IsDeleted { get; set; }

  DateTimeOffset? DeletedAt { get; set; }
}
