namespace AuditFlow.API.Domain.Common.Interfaces;

public interface IAuditableEntity
{
    string CreatedBy { get; set; }
    DateTimeOffset CreatedAt { get; set; }
    string UpdatedBy { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}
