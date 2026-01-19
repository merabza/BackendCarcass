using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace LibCrud;

public interface ICrud
{
    Task<OneOf<ICrudData, Err[]>> GetOne(int id, CancellationToken cancellationToken = default);

    Task<OneOf<ICrudData, Err[]>> Create(ICrudData crudDataForCreate, CancellationToken cancellationToken = default);

    Task<Option<Err[]>> Update(int id, ICrudData crudDataNewVersion, CancellationToken cancellationToken = default);

    Task<Option<Err[]>> Delete(int id, CancellationToken cancellationToken = default);
}
