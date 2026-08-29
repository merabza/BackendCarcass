using System.Threading;
using System.Threading.Tasks;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Crud;

public interface ICrud
{
    Task<Result<ICrudData>> GetOne(int id, CancellationToken cancellationToken = default);

    Task<Result<ICrudData>> Create(ICrudData crudDataForCreate, CancellationToken cancellationToken = default);

    Task<Result> Update(int id, ICrudData crudDataNewVersion, CancellationToken cancellationToken = default);

    Task<Result> Delete(int id, CancellationToken cancellationToken = default);
}
