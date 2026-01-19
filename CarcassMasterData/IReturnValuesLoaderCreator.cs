namespace CarcassMasterData;

public interface IReturnValuesLoaderCreator
{
    IReturnValuesLoader? CreateReturnValuesLoaderLoader(string tableName);
}
