using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.V1.Responses;
using CarcassRepositories.Models;

namespace CarcassRepositories;

public interface IMenuRightsRepository
{
    Task<MainMenuModel> MainMenu(string userName, CancellationToken cancellationToken);
    Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken);
    Task<DataTypesResponse[]> DataTypes(string userName, CancellationToken cancellationToken);
    Task<string?> GridModel(string dtKey, CancellationToken cancellationToken);
}