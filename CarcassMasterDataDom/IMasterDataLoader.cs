﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom;

public interface IMasterDataLoader
{
    Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken);
}