using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.LibCrud;

public interface ICrud
{
    Task<Result<ICrudData>> GetOne(int id, CancellationToken cancellationToken = default);

    Task<Result<ICrudData>> Create(ICrudData crudDataForCreate, CancellationToken cancellationToken = default);

    Task<Option<ErrorOmd[]>> Update(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default);

    Task<Option<ErrorOmd[]>> Delete(int id, CancellationToken cancellationToken = default);
}
