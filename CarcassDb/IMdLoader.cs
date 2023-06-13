using System.Linq;
using CarcassMasterDataDom;
using OneOf;
using SystemToolsShared;

namespace CarcassDb;

public interface IMdLoader
{
    OneOf<IQueryable<IDataType>, Err[]> Load();
}