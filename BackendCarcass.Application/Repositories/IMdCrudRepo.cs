using System.Threading.Tasks;
using BackendCarcass.Domain;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Repositories;

public interface IMdCrudRepo : IMdLoader
{
    Task<Result> Create(IDataType newItem);
    ValueTask<Result> Update(int id, IDataType newItem);
    ValueTask<Result> Delete(int id);
}
