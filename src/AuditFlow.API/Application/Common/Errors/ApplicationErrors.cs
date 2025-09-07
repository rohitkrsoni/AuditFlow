using FluentResults;

namespace AuditFlow.API.Application.Common.Errors;

public static class ApplicationErrors
{
    public static class Common
    {
        public static Error NotFound(string resource, object? id = null)
            => new Error($"{resource} with id '{id}' was not found.")
                .WithMetadata(ErrorMetadata.NotFound);
    }
}
