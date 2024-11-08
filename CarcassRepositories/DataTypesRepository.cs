using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;

namespace CarcassRepositories;

public class DataTypesRepository : IDataTypesRepository
{
    private readonly CarcassDbContext _context;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypesRepository(CarcassDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MenuToCrudTypeDomModel>> LoadMenuToCrudTypes(
        CancellationToken cancellationToken)
    {
        return await (from mmj in _context.ManyToManyJoins
                join pt in _context.DataTypes on mmj.PtId equals pt.DtId
                join ct in _context.DataTypes on mmj.CtId equals ct.DtId
                join p in _context.Menu on mmj.PKey equals p.MenKey
                join c in _context.CrudRightTypes on mmj.CKey equals c.CrtKey
                where pt.DtKey == ECarcassDataTypeKeys.MenuItm.ToDtKey() &&
                      ct.DtKey == ECarcassDataTypeKeys.CrudRightType.ToDtKey()
                select new MenuToCrudTypeDomModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.MenName + "." + c.CrtName))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataTypeToCrudTypeDomModel>> LoadDataTypesToCrudTypes(
        CancellationToken cancellationToken)
    {
        return await (from mmj in _context.ManyToManyJoins
            join pt in _context.DataTypes on mmj.PtId equals pt.DtId
            join ct in _context.DataTypes on mmj.CtId equals ct.DtId
            join p in _context.DataTypes on mmj.PKey equals p.DtKey
            join c in _context.CrudRightTypes on mmj.CKey equals c.CrtKey
            where pt.DtKey == ECarcassDataTypeKeys.DataType.ToDtKey() &&
                  ct.DtKey == ECarcassDataTypeKeys.CrudRightType.ToDtKey()
            select new DataTypeToCrudTypeDomModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.DtName + "." + c.CrtName,
                p.DtId)).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataTypeToDataTypeDomModel>> LoadDataTypesToDataTypes(
        CancellationToken cancellationToken)
    {
        var dataTypeKey = ECarcassDataTypeKeys.DataType.ToDtKey();
        return await (from mmj in _context.ManyToManyJoins
            join pt in _context.DataTypes on mmj.PtId equals pt.DtId
            join ct in _context.DataTypes on mmj.CtId equals ct.DtId
            join p in _context.DataTypes on mmj.PKey equals p.DtKey
            join c in _context.DataTypes on mmj.CKey equals c.DtKey
            where pt.DtKey == dataTypeKey && ct.DtKey == dataTypeKey
            select new DataTypeToDataTypeDomModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.DtName + "." + c.DtName,
                mmj.PKey)).ToListAsync(cancellationToken);
    }
}