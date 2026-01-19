using System.Threading.Tasks;
using CarcassMasterData;
using LanguageExt;
using SystemTools.SystemToolsShared.Errors;

namespace CarcassDb;

public interface IMdCrudRepo : IMdLoader
{
    Task<Option<Err[]>> Create(IDataType newItem);
    ValueTask<Option<Err[]>> Update(int id, IDataType newItem);
    ValueTask<Option<Err[]>> Delete(int id);
}
