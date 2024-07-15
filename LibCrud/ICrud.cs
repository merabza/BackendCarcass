using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using OneOf;
using SystemToolsShared.Errors;

namespace LibCrud;

public interface ICrud
{
    Task<OneOf<ICrudData, Err[]>> GetOne(int id, CancellationToken cancellationToken);
    Task<OneOf<ICrudData, Err[]>> Create(ICrudData crudDataForCreate, CancellationToken cancellationToken);
    Task<Option<Err[]>> Update(int id, ICrudData crudDataNewVersion, CancellationToken cancellationToken);
    Task<Option<Err[]>> Delete(int id, CancellationToken cancellationToken);
}