using System.Collections.Generic;
using LibCrud;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom;

public interface IMasterDataLoaderCreator
{
    OneOf<IMasterDataLoader, IEnumerable<Err>> CreateMasterDataLoader(string queryName);
    OneOf<CrudBase, IEnumerable<Err>> CreateMasterDataCrud(string tableName);
}