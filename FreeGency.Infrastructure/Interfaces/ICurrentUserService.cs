namespace FreeGency.Infrastructure.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string FirstName { get; }
    string LastName { get; }
    string origin { get; }
}
