using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace BackendCarcass.MasterData;

public sealed class MasterDataReturnValuesLoader : IReturnValuesLoader
{
    private readonly DataTypeModelForRvs _dt;
    private readonly IReturnValuesRepository _rvRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataReturnValuesLoader(DataTypeModelForRvs dt, IReturnValuesRepository rvRepo)
    {
        _dt = dt;
        _rvRepo = rvRepo;
    }

    public async Task<OneOf<IEnumerable<SrvModel>, Err[]>> GetSimpleReturnValues(
        CancellationToken cancellationToken = default)
    {
        return await _rvRepo.GetSimpleReturnValues(_dt, cancellationToken);
    }
}
