﻿using OneOf;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using SystemToolsShared;
using System.Threading;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace CarcassMasterDataDom;

public class MasterDataReturnValuesLoader : IReturnValuesLoader
{
    private readonly DataTypeModelForRvs _dt;
    private readonly IReturnValuesRepository _rvRepo;

    public MasterDataReturnValuesLoader(DataTypeModelForRvs dt, IReturnValuesRepository rvRepo)
    {
        _dt = dt;
        _rvRepo = rvRepo;
    }

    public async Task<OneOf<IEnumerable<SrvModel>, Err[]>> GetSimpleReturnValues(CancellationToken cancellationToken)
    {
        return await _rvRepo.GetSimpleReturnValues(_dt, cancellationToken);
    }

    
}