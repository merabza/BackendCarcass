using System.Collections.Generic;
using BackendCarcass.MasterData.Models;

namespace BackendCarcass.Application.MasterData.GetLookupTables;

public sealed record MdGetLookupTablesQueryResponse(Dictionary<string, IEnumerable<SrvModel>> Srv);