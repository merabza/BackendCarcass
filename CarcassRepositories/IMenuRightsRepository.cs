using System.Collections.Generic;
using System.Threading.Tasks;
using CarcassContracts.V1.Responses;
using CarcassDb.QueryModels;
using CarcassRepositories.Models;
using LanguageExt;
using SystemToolsShared;

namespace CarcassRepositories;

public interface IMenuRightsRepository
{
    Task<List<DataTypeModel>> ParentsTreeData(string userName, ERightsEditorViewStyle viewStyle);
    Task<List<DataTypeModel>> ChildrenTreeData(string userName, string dataTypeKey, ERightsEditorViewStyle viewStyle);
    Task<Option<Err[]>> OptimizeRights();

    Task<MainMenuModel> MainMenu(string userName);

    //Task<bool> CheckTableViewRight(IEnumerable<Claim> claims, string tableName);
    //Task<bool> CheckTableCrudRight(IEnumerable<Claim> claims, string tableName, ECrudOperationType crudType);
    //string? TableName<T>(); // where T : IDataType;
    //Task<bool> CheckUserRightToClaim(IEnumerable<Claim> userClaims, string claimName);
    Task<List<string>> UserAppClaims(string userName);
    Task<DataTypesResponse[]> DataTypes(string userName);
    Task<string?> GridModel(string dtKey);

    Task<List<TypeDataModel>> HalfChecks(string userName, int dataTypeId, string dataKey,
        ERightsEditorViewStyle viewStyle);

    Task<bool> SaveRightsChanges(string userName, List<RightsChangeModel> changedRights);
    //Task<bool> CheckUserToUserRight(string userName1, string userName2);
    //Task<bool> CheckUserAppClaimRight(string userName, string appClaimName);
}