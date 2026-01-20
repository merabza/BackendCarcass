using System.Linq;
using BackendCarcass.MasterData;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Db;

public interface IMdLoader
{
    OneOf<IQueryable<IDataType>, Err[]> Load();
}
