using System.Collections.Generic;
using CarcassMasterDataDom.Models;

namespace Carcass.Application.MasterData.GetLookupTables;

public sealed record MdGetLookupTablesQueryResponse(Dictionary<string, IEnumerable<SrvModel>> Srv);