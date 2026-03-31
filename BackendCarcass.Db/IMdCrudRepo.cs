using System.Threading.Tasks;
using BackendCarcass.MasterData;
using LanguageExt;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Db;

public interface IMdCrudRepo : IMdLoader
{
    Task<Option<Error[]>> Create(IDataType newItem);
    ValueTask<Option<Error[]>> Update(int id, IDataType newItem);
    ValueTask<Option<Error[]>> Delete(int id);
}
