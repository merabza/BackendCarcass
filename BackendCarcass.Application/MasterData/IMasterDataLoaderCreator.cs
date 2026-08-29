using BackendCarcass.Application.Crud;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData;

public interface IMasterDataLoaderCreator
{
    Result<IMasterDataLoader> CreateMasterDataLoader(string queryName);
    Result<CrudBase> CreateMasterDataCrud(string tableName);
}
