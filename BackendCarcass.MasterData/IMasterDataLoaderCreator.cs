using BackendCarcass.LibCrud;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

public interface IMasterDataLoaderCreator
{
    OneOf<IMasterDataLoader, Error[]> CreateMasterDataLoader(string queryName);
    OneOf<CrudBase, Error[]> CreateMasterDataCrud(string tableName);
}
