using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carcass.Database;
using Carcass.Database.Models;
using CarcassMasterData;
using CarcassMasterData.Models;
using DomainShared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarcassRepositories;

public sealed class DataTypesRepository : IDataTypesRepository
{
    private readonly CarcassDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypesRepository(CarcassDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MenuToCrudTypeDomModel>> LoadMenuToCrudTypes(
        CancellationToken cancellationToken = default)
    {
        return await (from mmj in _context.ManyToManyJoins
                join pt in _context.DataTypes on mmj.PtId equals pt.DtId
                join ct in _context.DataTypes on mmj.CtId equals ct.DtId
                join p in _context.Menu on mmj.PKey equals p.MenKey
                join c in _context.CrudRightTypes on mmj.CKey equals c.CrtKey
                where pt.DtTable == _unitOfWork.GetTableName<MenuItm>() &&
                      ct.DtTable == _unitOfWork.GetTableName<CrudRightType>()
                select new MenuToCrudTypeDomModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.MenName + "." + c.CrtName))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataTypeToCrudTypeDomModel>> LoadDataTypesToCrudTypes(
        CancellationToken cancellationToken = default)
    {
        return await (from mmj in _context.ManyToManyJoins
            join pt in _context.DataTypes on mmj.PtId equals pt.DtId
            join ct in _context.DataTypes on mmj.CtId equals ct.DtId
            join p in _context.DataTypes on mmj.PKey equals p.DtTable
            join c in _context.CrudRightTypes on mmj.CKey equals c.CrtKey
            where pt.DtTable == _unitOfWork.GetTableName<DataType>() &&
                  ct.DtTable == _unitOfWork.GetTableName<CrudRightType>()
            select new DataTypeToCrudTypeDomModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.DtName + "." + c.CrtName,
                p.DtId)).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataTypeToDataTypeDomModel>> LoadDataTypesToDataTypes(
        CancellationToken cancellationToken = default)
    {
        var dataTypeKey = _unitOfWork.GetTableName<DataType>();
        return await (from mmj in _context.ManyToManyJoins
            join pt in _context.DataTypes on mmj.PtId equals pt.DtId
            join ct in _context.DataTypes on mmj.CtId equals ct.DtId
            join p in _context.DataTypes on mmj.PKey equals p.DtTable
            join c in _context.DataTypes on mmj.CKey equals c.DtTable
            where pt.DtTable == dataTypeKey && ct.DtTable == dataTypeKey
            select new DataTypeToDataTypeDomModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.DtName + "." + c.DtName,
                mmj.PKey)).ToListAsync(cancellationToken);
    }
}