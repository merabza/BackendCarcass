namespace CarcassMasterDataDom;

public interface IReturnValuesLoaderCreator
{
    IReturnValuesLoader? CreateReturnValuesLoaderLoader(string tableName);
}