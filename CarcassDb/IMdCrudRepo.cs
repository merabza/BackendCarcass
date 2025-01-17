using System.Collections.Generic;
using System.Threading.Tasks;
using CarcassMasterDataDom;
using LanguageExt;
using SystemToolsShared.Errors;

namespace CarcassDb;

public interface IMdCrudRepo : IMdLoader
{
    Task<Option<IEnumerable<Err>>> Create(IDataType newItem);
    ValueTask<Option<IEnumerable<Err>>> Update(int id, IDataType newItem);
    ValueTask<Option<IEnumerable<Err>>> Delete(int id);
}