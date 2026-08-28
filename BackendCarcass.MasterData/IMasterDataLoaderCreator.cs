using BackendCarcass.LibCrud;
using SystemTools.SharedKernel;

namespace BackendCarcass.MasterData;

public interface IMasterDataLoaderCreator
{
    Result<IMasterDataLoader> CreateMasterDataLoader(string queryName);
    Result<CrudBase> CreateMasterDataCrud(string tableName);
}
