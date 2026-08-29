using System.Linq;
using BackendCarcass.Domain;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Repositories;

public interface IMdLoader
{
    Result<IQueryable<IDataType>> Load();
}
