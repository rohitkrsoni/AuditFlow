namespace AuditFlow.API.Infrastructure.Auth;

public static class InternalRole
{
    private const string SuperUser = "SuperUser";
    private const string Admin = "Admin";
    private const string SuperVisor = "SuperVisor";
    private const string Editor = "Editor";

    public static IReadOnlyList<string> GetRoles =>
    [
        SuperUser,
        Admin,
        SuperVisor,
        Editor
    ];
}
