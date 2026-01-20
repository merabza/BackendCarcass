using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Database;
using BackendCarcass.Database.Models;
using Microsoft.EntityFrameworkCore;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.Repositories;

public class UserClaimsRepository : IUserClaimsRepository
{
    private readonly CarcassDbContext _carcassContext;
    private readonly IUnitOfWork _unitOfWork;

    public UserClaimsRepository(CarcassDbContext carcassContext, IUnitOfWork unitOfWork)
    {
        _carcassContext = carcassContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken = default)
    {
        var userDataTypeId = await DataTypeIdByKey(_unitOfWork.GetTableName<User>(), cancellationToken);
        var roleDataTypeId = await DataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var appClaimDataTypeId = await DataTypeIdByKey(_unitOfWork.GetTableName<AppClaim>(), cancellationToken);

        return await Task.FromResult(ManyToManyJoinsPcc(userDataTypeId, userName, roleDataTypeId, appClaimDataTypeId)
            .ToList());
    }

    private Task<int> DataTypeIdByKey(string tableName, CancellationToken cancellationToken = default)
    {
        return _carcassContext.DataTypes.Where(w => w.DtTable == tableName).Select(s => s.DtId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IEnumerable<string> ManyToManyJoinsPcc(int parentTypeId, string parentKey, int childTypeId,
        int childTypeId2)
    {
        return from r1 in _carcassContext.ManyToManyJoins
            join r2 in _carcassContext.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals new
            {
                t = r2.PtId, i = r2.PKey
            }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId && r2.CtId == childTypeId2
            select r2.CKey;
    }
}