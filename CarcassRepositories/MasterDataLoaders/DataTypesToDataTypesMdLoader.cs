using System.Linq;
using CarcassDb;
using CarcassDb.QueryModels;
using CarcassMasterDataDom;
using OneOf;
using SystemToolsShared;

namespace CarcassRepositories.MasterDataLoaders;

public sealed class DataTypesToDataTypesMdLoader : IMdLoader
{
    private readonly CarcassDbContext _context;
    //private readonly IDataTypeKeys _dataTypeKeys;

    public DataTypesToDataTypesMdLoader(CarcassDbContext ctx)
    {
        _context = ctx;
        //_dataTypeKeys = dataTypeKeys;
    }

    public OneOf<IQueryable<IDataType>, Err[]> Load()
    {
        var dataTpeKey = ECarcassDataTypeKeys.DataType.ToDtKey();
        return OneOf<IQueryable<IDataType>, Err[]>.FromT0(
            from mmj in _context.ManyToManyJoins
            join pt in _context.DataTypes on mmj.PtId equals pt.DtId
            join ct in _context.DataTypes on mmj.CtId equals ct.DtId
            join p in _context.DataTypes on mmj.PKey equals p.DtKey
            join c in _context.DataTypes on mmj.CKey equals c.DtKey
            where pt.DtKey == dataTpeKey && ct.DtKey == dataTpeKey
            select new DataTypeToDataTypeModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.DtName + "." + c.DtName,
                mmj.PKey));
    }
}