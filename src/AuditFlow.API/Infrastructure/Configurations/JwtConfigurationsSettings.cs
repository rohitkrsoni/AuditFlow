namespace AuditFlow.API.Infrastructure.Configurations;

internal sealed record JwtConfigurationsSettings(string Audience, string Issuer, string SecretKey);
