using LibCrud;

namespace CarcassMasterDataDom;

public interface IMasterDataLoaderCreator
{
    IMasterDataLoader CreateMasterDataLoader(string queryName);
    CrudBase CreateMasterDataCrud(string tableName);
}