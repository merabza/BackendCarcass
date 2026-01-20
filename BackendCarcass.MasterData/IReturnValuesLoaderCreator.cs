namespace BackendCarcass.MasterData;

public interface IReturnValuesLoaderCreator
{
    IReturnValuesLoader? CreateReturnValuesLoaderLoader(string tableName);
}