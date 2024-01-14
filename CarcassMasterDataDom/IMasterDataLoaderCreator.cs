using LibCrud;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom;

public interface IMasterDataLoaderCreator
{
    OneOf<IMasterDataLoader, Err[]> CreateMasterDataLoader(string queryName);
    OneOf<CrudBase, Err[]> CreateMasterDataCrud(string tableName);
}