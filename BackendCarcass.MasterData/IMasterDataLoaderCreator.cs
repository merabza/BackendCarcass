using BackendCarcass.LibCrud;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

public interface IMasterDataLoaderCreator
{
    OneOf<IMasterDataLoader, ErrorOmd[]> CreateMasterDataLoader(string queryName);
    OneOf<CrudBase, ErrorOmd[]> CreateMasterDataCrud(string tableName);
}
