using System.Threading.Tasks;
using BackendCarcassDomain.Entities;
using LanguageExt;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public interface IMdCrudRepo : IMdLoader
{
    Task<Option<Error[]>> Create(IDataType newItem);
    ValueTask<Option<Error[]>> Update(int id, IDataType newItem);
    ValueTask<Option<Error[]>> Delete(int id);
}
