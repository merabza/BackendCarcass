using LibCrud;

namespace CarcassMasterDataDom;

public interface IMasterDataLoaderCrudCreator
{
    IMasterDataLoader CreateMasterDataLoader(string queryName);
    CrudBase CreateMasterDataCrud(string tableName);
}