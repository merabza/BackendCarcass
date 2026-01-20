using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackendCarcass.Repositories;

public interface IUserClaimsRepository
{
    Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken = default);
}