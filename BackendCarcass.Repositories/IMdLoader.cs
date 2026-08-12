using System.Linq;
using BackendCarcassDomain.Entities;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public interface IMdLoader
{
    OneOf<IQueryable<IDataType>, Error[]> Load();
}
