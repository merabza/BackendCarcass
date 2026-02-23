using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Repositories.Models;
using BackendCarcassShared.BackendCarcassContracts.V1.Responses;

namespace BackendCarcass.Repositories;

public interface IMenuRightsRepository
{
    Task<MainMenuModel> MainMenu(string userName, CancellationToken cancellationToken = default);

    //Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken = default);
    Task<DataTypesResponse[]> DataTypes(string userName, CancellationToken cancellationToken = default);
    Task<string?> GridModel(string tableName, CancellationToken cancellationToken = default);
}
