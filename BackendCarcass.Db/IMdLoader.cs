using System.Linq;
using BackendCarcass.MasterData;
using BackendCarcassDomain.Entities;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Db;

public interface IMdLoader
{
    OneOf<IQueryable<IDataType>, Error[]> Load();
}
