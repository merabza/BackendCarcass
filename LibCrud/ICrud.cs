using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using OneOf;
using SystemToolsShared.Errors;

namespace LibCrud;

public interface ICrud
{
    Task<OneOf<ICrudData, IEnumerable<Err>>> GetOne(int id, CancellationToken cancellationToken = default);

    Task<OneOf<ICrudData, IEnumerable<Err>>> Create(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default);

    Task<Option<IEnumerable<Err>>> Update(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default);

    Task<Option<IEnumerable<Err>>> Delete(int id, CancellationToken cancellationToken = default);
}