using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using SystemTools.SharedKernel;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace BackendCarcass.Application.MasterData;

public sealed class MasterDataReturnValuesLoader : IReturnValuesLoader
{
    private readonly DataTypeModelForRvs _dt;
    private readonly IReturnValuesRepository _rvRepo;

    public MasterDataReturnValuesLoader(DataTypeModelForRvs dt, IReturnValuesRepository rvRepo)
    {
        _dt = dt;
        _rvRepo = rvRepo;
    }

    public async Task<Result<IEnumerable<SrvModel>>> GetSimpleReturnValues(
        CancellationToken cancellationToken = default)
    {
        return await _rvRepo.GetSimpleReturnValues(_dt, cancellationToken);
    }
}
