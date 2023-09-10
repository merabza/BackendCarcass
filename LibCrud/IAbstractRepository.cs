using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibCrud;

public interface IAbstractRepository
{
    Task<IDbContextTransaction> GetTransaction(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
    string? GetTableName<T>();
}