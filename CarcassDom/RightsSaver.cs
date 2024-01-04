using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassDom.Models;
using CarcassMasterDataDom;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToPrimaryConstructor

namespace CarcassDom;

public class RightsSaver
{
    private readonly ILogger _logger;
    private readonly IRightsRepository _repo;

    public RightsSaver(ILogger logger, IRightsRepository repo)
    {
        _logger = logger;
        _repo = repo;
    }

    public async Task<bool> SaveRightsChanges(string userName, List<RightsChangeModel> changedRights,
        CancellationToken cancellationToken)
    {
        try
        {
            var dtDataId = await _repo.DataTypeIdByKey(ECarcassDataTypeKeys.DataType, cancellationToken);
            var mmjDataId = await _repo.DataTypeIdByKey(ECarcassDataTypeKeys.DataTypeToDataType, cancellationToken);
            var roleDataId = await _repo.DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
            var userDataId = await _repo.DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);
            var allowPairs = await _repo.ManyToManyJoinsPcc4(userDataId, userName, roleDataId, mmjDataId, dtDataId,
                dtDataId, cancellationToken);

            foreach (var drr in changedRights)
            {
                if (drr.Parent is null || drr.Child is null)
                    throw new Exception("SaveRightsChanges: parent or child keys are not valid");

                var parentKey = await _repo.DataTypeKeyById(drr.Parent.DtId, cancellationToken);
                var childKey = await _repo.DataTypeKeyById(drr.Child.DtId, cancellationToken);

                if (parentKey is null || childKey is null)
                    throw new Exception("SaveRightsChanges: parent or child keys are not valid");

                if (!allowPairs.Contains(new Tuple<string, string>(parentKey, childKey)))
                    continue;

                var mmj = await _repo.GetOneManyToManyJoin(drr.Parent.DtId, drr.Parent.DKey, drr.Child.DtId,
                    drr.Child.DKey,
                    cancellationToken);

                if (mmj == null && drr.Checked)
                {
                    if (!await _repo.CreateAndSaveOneManyToManyJoin(drr.Parent.DtId, drr.Parent.DKey, drr.Child.DtId,
                            drr.Child.DKey, cancellationToken))
                        return false;
                }
                else if (mmj != null && !drr.Checked)
                {
                    if (!await _repo.RemoveOneManyToManyJoin(mmj, cancellationToken))
                        return false;
                }
            }

            await _repo.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }
}