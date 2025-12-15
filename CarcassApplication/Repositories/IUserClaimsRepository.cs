using RepositoriesDom;

namespace CarcassApplication.Repositories;

public interface IUserClaimsRepository : IAbstractRepository
{
    Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken = default);
}