using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDom.Models;
using CarcassMasterDataDom.Models;
using RepositoriesDom;

namespace CarcassDom;

public interface IRightsRepository : IAbstractRepository
{
    Task<int> DataTypeIdByTableName(string tableName, CancellationToken cancellationToken = default);
    int UserMinLevel(IEnumerable<string> drPcs);

    Task<List<Tuple<int, int>>> UsersMinLevels(int roleDataId, int userDataId,
        CancellationToken cancellationToken = default);

    Task<List<ReturnValueModel>> GetRoleReturnValues(int minLevel, CancellationToken cancellationToken = default);

    Task<List<DataTypeModelForRvs>> ParentsDataTypesNormalView(int dtDataId, string dataTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken = default);

    IQueryable<string> ManyToManyJoinsPc(int parentTypeId, string parentKey, int childTypeId);

    Task<List<DataTypeModelForRvs>> ParentsDataTypesReverseView(int dtDataId, int userDataId, string userName,
        int roleDataId, int mmjDataId, CancellationToken cancellationToken = default);

    Task<List<DataTypeModelForRvs>> ChildrenDataTypesNormalView(int dtDataId, string parentTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken = default);

    Task<List<DataTypeModelForRvs>> ChildrenDataTypesReverseView(int dtDataId, string parentTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken = default);

    Task<List<UserModel>> GetUsers(CancellationToken cancellationToken = default);

    Task<List<TypeDataModel>> HalfChecksNormalView(int userDataId, string userName, int roleDataId, int mmjDataId,
        int dtDataId, int dataTypeId, string dataKey, CancellationToken cancellationToken = default);

    Task<List<TypeDataModel>> HalfChecksReverseView(int userDataId, string userName, int roleDataId, int mmjDataId,
        int dtDataId, int dataTypeId, string dataKey, CancellationToken cancellationToken = default);

    Task<List<Tuple<string, string>>> ManyToManyJoinsPcc4(int parentTypeId, string parentKey, int childTypeId,
        int mmjDataId, int childTypeId2, int childTypeId3, CancellationToken cancellationToken = default);

    Task<string?> DataTypeKeyById(int dtId, CancellationToken cancellationToken = default);

    Task<ManyToManyJoinModel?> GetOneManyToManyJoin(int parentDtId, string parentDKey, int childDtId, string childDKey,
        CancellationToken cancellationToken = default);

    Task<bool> CreateAndSaveOneManyToManyJoin(int parentDtId, string parentDKey, int childDtId, string childDKey,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveOneManyToManyJoin(ManyToManyJoinModel manyToManyJoinModel,
        CancellationToken cancellationToken = default);
}