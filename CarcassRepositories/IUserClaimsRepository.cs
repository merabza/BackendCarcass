using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RepositoriesAbstraction;

namespace CarcassRepositories;

public interface IUserClaimsRepository : IAbstractRepository
{
    Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken = default);
}