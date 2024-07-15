using System.Threading.Tasks;
using CarcassMasterDataDom;
using LanguageExt;
using SystemToolsShared.Errors;

namespace CarcassDb;

public interface IMdCrudRepo : IMdLoader
{
    Task<Option<Err[]>> Create(IDataType newItem);
    Task<Option<Err[]>> Update(int id, IDataType newItem);
    Task<Option<Err[]>> Delete(int id);
}