using CarcassMasterDataDom;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using CarcassMasterDataDom.Models;
using System.Linq;
using CarcassDom.Models;

namespace CarcassDom;

public interface IRightsRepository
{
    Task<int> DataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey, CancellationToken cancellationToken);
    int UserMinLevel(IEnumerable<string> drPcs);
    Task<List<Tuple<int, int>>> UsersMinLevels(int roleDataId, int userDataId, CancellationToken cancellationToken);

    Task<List<ReturnValueModel>> GetRoleReturnValues(int minLevel, CancellationToken cancellationToken);

    Task<List<DataTypeModelForRvs>> ParentsDataTypesNormalView(int dtDataId, string dataTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken);

    IQueryable<string> ManyToManyJoinsPc(int parentTypeId, string parentKey, int childTypeId);
    IQueryable<string> ManyToManyJoinsPcc2(int parentTypeId, string parentKey, int childTypeId, int mmjDataId,
        int childTypeId2, int childTypeId3);

    Task<List<DataTypeModelForRvs>> ParentsDataTypesReverseView(int dtDataId, int userDataId, string userName, int roleDataId,
        int mmjDataId, CancellationToken cancellationToken);

    Task<List<DataTypeModelForRvs>> ChildrenDataTypesNormalView(int dtDataId, string parentTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken);

    Task<List<DataTypeModelForRvs>> ChildrenDataTypesReverseView(int dtDataId, string parentTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken);

    Task<List<UserModel>> GetUsers(CancellationToken cancellationToken);

}