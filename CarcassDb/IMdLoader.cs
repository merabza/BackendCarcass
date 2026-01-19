using System.Linq;
using CarcassMasterData;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace CarcassDb;

public interface IMdLoader
{
    OneOf<IQueryable<IDataType>, Err[]> Load();
}
