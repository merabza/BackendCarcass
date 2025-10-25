using LibCrud;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom;

public interface IMasterDataLoaderCreator
{
    OneOf<IMasterDataLoader, Err[]> CreateMasterDataLoader(string queryName);
    OneOf<CrudBase, Err[]> CreateMasterDataCrud(string tableName);
}