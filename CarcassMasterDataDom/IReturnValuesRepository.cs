﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;

namespace CarcassMasterDataDom;

public interface IReturnValuesRepository
{
    Task<List<DataTypeModelForRvs>> GetDataTypesByTableNames(List<string> tableNames,
        CancellationToken cancellationToken);

    Task<List<ReturnValueModel>> GetAllReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken);
    Task<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken);
}