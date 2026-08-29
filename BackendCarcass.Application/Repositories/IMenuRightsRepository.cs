using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Repositories.Models;
using BackendCarcassShared.Contracts.V1.Responses;

namespace BackendCarcass.Application.Repositories;

public interface IMenuRightsRepository
{
    Task<MainMenuModel> MainMenu(string userName, CancellationToken cancellationToken = default);

    //Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken = default);
    Task<DataTypesResponse[]> DataTypes(string userName, CancellationToken cancellationToken = default);
    Task<string?> GridModel(string tableName, CancellationToken cancellationToken = default);
}
