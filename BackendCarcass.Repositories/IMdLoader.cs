using System.Linq;
using BackendCarcassDomain.Entities;
using SystemTools.SharedKernel;

namespace BackendCarcass.Repositories;

public interface IMdLoader
{
    Result<IQueryable<IDataType>> Load();
}
