﻿using CarcassMasterDataDom;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using CarcassMasterDataDom.Models;
using System.Linq;
using CarcassDom.Models;
using LibCrud;

namespace CarcassDom;

public interface IRightsRepository : IAbstractRepository
{
    Task<int> DataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey, CancellationToken cancellationToken);
    int UserMinLevel(IEnumerable<string> drPcs);
    Task<List<Tuple<int, int>>> UsersMinLevels(int roleDataId, int userDataId, CancellationToken cancellationToken);

    Task<List<ReturnValueModel>> GetRoleReturnValues(int minLevel, CancellationToken cancellationToken);

    Task<List<DataTypeModelForRvs>> ParentsDataTypesNormalView(int dtDataId, string dataTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken);

    IQueryable<string> ManyToManyJoinsPc(int parentTypeId, string parentKey, int childTypeId);

    Task<List<DataTypeModelForRvs>> ParentsDataTypesReverseView(int dtDataId, int userDataId, string userName, int roleDataId,
        int mmjDataId, CancellationToken cancellationToken);

    Task<List<DataTypeModelForRvs>> ChildrenDataTypesNormalView(int dtDataId, string parentTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken);

    Task<List<DataTypeModelForRvs>> ChildrenDataTypesReverseView(int dtDataId, string parentTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken);

    Task<List<UserModel>> GetUsers(CancellationToken cancellationToken);

    Task<List<TypeDataModel>> HalfChecksNormalView(int userDataId, string userName, int roleDataId, int mmjDataId,
        int dtDataId, int dataTypeId, string dataKey, CancellationToken cancellationToken);

    Task<List<TypeDataModel>> HalfChecksReverseView(int userDataId, string userName, int roleDataId, int mmjDataId,
        int dtDataId, int dataTypeId, string dataKey, CancellationToken cancellationToken);

    IQueryable<Tuple<string, string>> ManyToManyJoinsPcc4(int parentTypeId, string parentKey, int childTypeId,
        int mmjDataId, int childTypeId2, int childTypeId3);

    Task<string?> DataTypeKeyById(int dtId, CancellationToken cancellationToken);

    Task<ManyToManyJoinModel?> GetOneManyToManyJoin(int parentDtId, string parentDKey, int childDtId, string childDKey,
        CancellationToken cancellationToken);

    Task<bool> CreateAndSaveOneManyToManyJoin(int parentDtId, string parentDKey, int childDtId, string childDKey,
        CancellationToken cancellationToken);

    Task<bool> RemoveOneManyToManyJoin(ManyToManyJoinModel manyToManyJoinModel, CancellationToken cancellationToken);


}