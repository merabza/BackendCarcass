using LibCrud;

namespace CarcassMasterDataDom;

public interface IMasterDataLoaderCrudCreator
{
    IMasterDataLoader CreateMasterDataLoader(string tableName);
    CrudBase CreateMasterDataCrud(string tableName);
}