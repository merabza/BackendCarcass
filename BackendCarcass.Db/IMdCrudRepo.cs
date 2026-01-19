using System.Threading.Tasks;
using BackendCarcass.MasterData;
using LanguageExt;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Db;

public interface IMdCrudRepo : IMdLoader
{
    Task<Option<Err[]>> Create(IDataType newItem);
    ValueTask<Option<Err[]>> Update(int id, IDataType newItem);
    ValueTask<Option<Err[]>> Delete(int id);
}
