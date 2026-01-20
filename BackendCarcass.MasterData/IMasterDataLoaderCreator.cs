using BackendCarcass.LibCrud;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

public interface IMasterDataLoaderCreator
{
    OneOf<IMasterDataLoader, Err[]> CreateMasterDataLoader(string queryName);
    OneOf<CrudBase, Err[]> CreateMasterDataCrud(string tableName);
}