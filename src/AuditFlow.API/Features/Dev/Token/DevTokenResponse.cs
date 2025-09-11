namespace AuditFlow.API.Features.Dev.Token;

public sealed record DevTokenResponse(string Access_Token, string Token_Type, int Expires_In);
