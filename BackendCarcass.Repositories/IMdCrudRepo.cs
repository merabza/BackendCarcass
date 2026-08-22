using System.Threading.Tasks;
using BackendCarcassDomain.Entities;
using LanguageExt;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public interface IMdCrudRepo : IMdLoader
{
    Task<Option<ErrorOmd[]>> Create(IDataType newItem);
    ValueTask<Option<ErrorOmd[]>> Update(int id, IDataType newItem);
    ValueTask<Option<ErrorOmd[]>> Delete(int id);
}
