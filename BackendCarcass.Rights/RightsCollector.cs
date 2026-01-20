using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Database.Models;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using BackendCarcass.Rights.Models;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SystemTools.DomainShared.Repositories;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace BackendCarcass.Rights;

public sealed class RightsCollector
{
    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RightsCollector(IRightsRepository repo, IReturnValuesRepository rvRepo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<OneOf<List<DataTypeModel>, Err[]>> ParentsTreeData(string userName,
        ERightsEditorViewStyle viewStyle, CancellationToken cancellationToken = default)
    {
        var dataTypes =
            (viewStyle == ERightsEditorViewStyle.NormalView
                ? await ParentsDataTypesNormalView(userName, cancellationToken)
                : await ParentsDataTypesReverseView(userName, cancellationToken)).OrderBy(o => o.DtTable)
            .GroupBy(g => g.DtId).Select(s => s.First());

        return await GetTreeData(userName, dataTypes, cancellationToken);
    }

    private async ValueTask<OneOf<List<DataTypeModel>, Err[]>> GetTreeData(string userName,
        IEnumerable<DataTypeModelForRvs> dataTypes, CancellationToken cancellationToken = default)
    {
        var dataTypeModels = new List<DataTypeModel>();
        var errors = new List<Err>();
        foreach (var dataType in dataTypes)
        {
            var dataTypeModel = new DataTypeModel(dataType.DtId, dataType.DtTable, dataType.DtName,
                dataType.DtParentDataTypeId);
            var entResult = await GetRetValues(dataType, userName, cancellationToken);

            if (entResult.IsT1)
                errors.AddRange(entResult.AsT1);
            else
                dataTypeModel.ReturnValues = entResult.AsT0;

            dataTypeModels.Add(dataTypeModel);
        }

        if (errors.Count > 0) return errors.ToArray();

        return dataTypeModels;
    }

    public async Task<OneOf<List<DataTypeModel>, Err[]>> ChildrenTreeData(string userName, string dataTypeKey,
        ERightsEditorViewStyle viewStyle, CancellationToken cancellationToken = default)
    {
        var dataTypes =
            (viewStyle == ERightsEditorViewStyle.NormalView
                ? await ChildrenDataTypesNormalView(userName, dataTypeKey, cancellationToken)
                : await ChildrenDataTypesReverseView(userName, dataTypeKey, cancellationToken)).OrderBy(o => o.DtName)
            .GroupBy(g => g.DtId).Select(s => s.First()).ToList();

        return await GetTreeData(userName, dataTypes, cancellationToken);
    }

    private async Task<List<DataTypeModelForRvs>> ParentsDataTypesNormalView(string userName,
        CancellationToken cancellationToken = default)
    {
        var dataTypeTableName = _unitOfWork.GetTableName<DataType>();
        var dtDataId = await _repo.DataTypeIdByTableName(dataTypeTableName, cancellationToken);
        var mmjDataId = await _repo.DataTypeIdByTableName($"{dataTypeTableName}{dataTypeTableName}", cancellationToken);
        var roleDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var userDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<User>(), cancellationToken);

        return await _repo.ParentsDataTypesNormalView(dtDataId, dataTypeTableName, userDataId, userName, roleDataId,
            mmjDataId, cancellationToken);
    }

    private async Task<List<DataTypeModelForRvs>> ParentsDataTypesReverseView(string userName,
        CancellationToken cancellationToken = default)
    {
        var dataTypeTableName = _unitOfWork.GetTableName<DataType>();
        var dtDataId = await _repo.DataTypeIdByTableName(dataTypeTableName, cancellationToken);
        var mmjDataId = await _repo.DataTypeIdByTableName($"{dataTypeTableName}{dataTypeTableName}", cancellationToken);
        var roleDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var userDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<User>(), cancellationToken);

        return await _repo.ParentsDataTypesReverseView(dtDataId, userDataId, userName, roleDataId, mmjDataId,
            cancellationToken);
    }

    private async Task<List<DataTypeModelForRvs>> ChildrenDataTypesNormalView(string userName, string parentTypeKey,
        CancellationToken cancellationToken = default)
    {
        var dataTypeTableName = _unitOfWork.GetTableName<DataType>();
        var dtDataId = await _repo.DataTypeIdByTableName(dataTypeTableName, cancellationToken);
        var mmjDataId = await _repo.DataTypeIdByTableName($"{dataTypeTableName}{dataTypeTableName}", cancellationToken);
        var roleDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var userDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<User>(), cancellationToken);

        return await _repo.ChildrenDataTypesNormalView(dtDataId, parentTypeKey, userDataId, userName, roleDataId,
            mmjDataId, cancellationToken);
    }

    private async Task<List<DataTypeModelForRvs>> ChildrenDataTypesReverseView(string userName, string parentTypeKey,
        CancellationToken cancellationToken = default)
    {
        var dataTypeTableName = _unitOfWork.GetTableName<DataType>();
        var dtDataId = await _repo.DataTypeIdByTableName(dataTypeTableName, cancellationToken);
        var mmjDataId = await _repo.DataTypeIdByTableName($"{dataTypeTableName}{dataTypeTableName}", cancellationToken);
        var roleDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var userDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<User>(), cancellationToken);

        return await _repo.ChildrenDataTypesReverseView(dtDataId, parentTypeKey, userDataId, userName, roleDataId,
            mmjDataId, cancellationToken);
    }

    private async Task<OneOf<List<ReturnValueModel>, Err[]>> GetRetValues(DataTypeModelForRvs dt, string userName,
        CancellationToken cancellationToken = default)
    {
        if (dt.DtTable == _unitOfWork.GetTableName<User>())
        {
            var minOfLevel = await UserMinLevel(userName, cancellationToken);
            var uml = await UsersMinLevels(cancellationToken);

            var users = await _repo.GetUsers(cancellationToken);

            return (from usr in users
                    join ml in uml on usr.UsrId equals ml.Item1 into gj
                    from s in gj.DefaultIfEmpty()
                    where s != null && s.Item2 >= minOfLevel
                    select new ReturnValueModel { Id = usr.UsrId, Key = usr.NormalizedUserName, Name = usr.FullName })
                .ToList();
        }

        if (dt.DtTable != _unitOfWork.GetTableName<Role>())
            return await _rvRepo.GetAllReturnValues(dt, cancellationToken);

        var minLevel = await UserMinLevel(userName, cancellationToken);
        return await _repo.GetRoleReturnValues(minLevel, cancellationToken);
    }

    private async Task<int> UserMinLevel(string userName, CancellationToken cancellationToken = default)
    {
        var roleDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var userDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<User>(), cancellationToken);

        var drPcs = await _repo.ManyToManyJoinsPc(userDataId, userName, roleDataId).ToListAsync(cancellationToken);

        return _repo.UserMinLevel(drPcs);
    }

    private async Task<List<Tuple<int, int>>> UsersMinLevels(CancellationToken cancellationToken = default)
    {
        var roleDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var userDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<User>(), cancellationToken);

        return await _repo.UsersMinLevels(roleDataId, userDataId, cancellationToken);
    }

    public async Task<List<TypeDataModel>> HalfChecks(string userName, int dataTypeId, string dataKey,
        ERightsEditorViewStyle viewStyle, CancellationToken cancellationToken = default)
    {
        var dataTypeTableName = _unitOfWork.GetTableName<DataType>();
        var dtDataId = await _repo.DataTypeIdByTableName(dataTypeTableName, cancellationToken);
        var mmjDataId = await _repo.DataTypeIdByTableName($"{dataTypeTableName}{dataTypeTableName}", cancellationToken);
        var roleDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var userDataId = await _repo.DataTypeIdByTableName(_unitOfWork.GetTableName<User>(), cancellationToken);

        if (viewStyle == ERightsEditorViewStyle.NormalView)
            return await _repo.HalfChecksNormalView(userDataId, userName, roleDataId, mmjDataId, dtDataId, dataTypeId,
                dataKey, cancellationToken);

        return await _repo.HalfChecksReverseView(userDataId, userName, roleDataId, mmjDataId, dtDataId, dataTypeId,
            dataKey, cancellationToken);
    }
}