namespace FreeGency.Infrastructure.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string origin { get; }
   Task< Guid> GetProfileId(Guid userid);
}
