namespace AuditFlow.Shared.AuditContracts;

public enum AuditTransactionTypes
{
    Unknown = 0,
    Insert = 1,
    Update = 2,
    Delete = 3,
    SoftDelete = 4
}
