namespace BackendCarcass.Application.MasterData;

public interface IReturnValuesLoaderCreator
{
    IReturnValuesLoader? CreateReturnValuesLoaderLoader(string tableName);
}
