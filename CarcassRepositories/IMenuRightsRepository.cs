using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.V1.Responses;
using CarcassDom;
using CarcassDom.Models;
using CarcassRepositories.Models;

namespace CarcassRepositories;

public interface IMenuRightsRepository
{
    //Task<List<DataTypeModel>> ParentsTreeData(string userName, ERightsEditorViewStyle viewStyle,
    //    CancellationToken cancellationToken);

    Task<List<DataTypeModel>> ChildrenTreeData(string userName, string dataTypeKey, ERightsEditorViewStyle viewStyle,
        CancellationToken cancellationToken);

    //Task<Option<Err[]>> OptimizeRights(CancellationToken cancellationToken);
    Task<MainMenuModel> MainMenu(string userName, CancellationToken cancellationToken);
    Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken);
    Task<DataTypesResponse[]> DataTypes(string userName, CancellationToken cancellationToken);
    Task<string?> GridModel(string dtKey, CancellationToken cancellationToken);

    Task<List<TypeDataModel>> HalfChecks(string userName, int dataTypeId, string dataKey,
        ERightsEditorViewStyle viewStyle, CancellationToken cancellationToken);

    Task<bool> SaveRightsChanges(string userName, List<RightsChangeModel> changedRights,
        CancellationToken cancellationToken);
}