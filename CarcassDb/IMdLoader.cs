using System.Linq;
using CarcassMasterDataDom;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassDb;

public interface IMdLoader
{
    OneOf<IQueryable<IDataType>, Err[]> Load();
}