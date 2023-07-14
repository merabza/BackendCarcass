using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibCrud;

public interface IAbstractRepository
{
    IDbContextTransaction GetTransaction();
    Task SaveChangesAsync();
    string? GetTableName<T>();
}