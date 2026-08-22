using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.LibCrud;

public interface ICrud
{
    Task<OneOf<ICrudData, ErrorOmd[]>> GetOne(int id, CancellationToken cancellationToken = default);

    Task<OneOf<ICrudData, ErrorOmd[]>> Create(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default);

    Task<Option<ErrorOmd[]>> Update(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default);

    Task<Option<ErrorOmd[]>> Delete(int id, CancellationToken cancellationToken = default);
}
