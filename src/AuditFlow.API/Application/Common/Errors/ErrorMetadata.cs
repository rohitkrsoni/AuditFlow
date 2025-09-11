namespace AuditFlow.API.Application.Common.Errors;

internal static class ErrorMetadata
{
    public static readonly Dictionary<string, object> NotFound =
        new() { { ErrorMetadataType.NotFound, "Not Found Issue" } };
    public static readonly Dictionary<string, object> Validation =
        new() { { ErrorMetadataType.Validation, "Validation Issue" } };
}
