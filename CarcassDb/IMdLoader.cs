using CarcassMasterDataDom;
using OneOf;
using System.Linq;
using SystemToolsShared.Errors;

namespace CarcassDb;

public interface IMdLoader
{
    OneOf<IQueryable<IDataType>, Err[]> Load();
}