namespace AuditFlow.API.Infrastructure.Services;

public interface ICurrentUserService
{
    string IdentityId { get; }
}

public class DefaultCurrentUserService : ICurrentUserService
{
    public DefaultCurrentUserService()
    {
        IdentityId = "DefaultUser";
    }

    public string IdentityId { get; }
}
