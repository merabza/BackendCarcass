using System.Linq;
using CarcassMasterData;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassDb;

public interface IMdLoader
{
    OneOf<IQueryable<IDataType>, Err[]> Load();
}