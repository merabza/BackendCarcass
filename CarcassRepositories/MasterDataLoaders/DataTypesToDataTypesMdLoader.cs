//using System.Linq;
//using CarcassDb;
//using CarcassDb.QueryModels;
//using CarcassMasterDataDom;
//using OneOf;
//using SystemToolsShared;

//namespace CarcassRepositories.MasterDataLoaders;

//public sealed class DataTypesToDataTypesMdLoader(CarcassDbContext ctx) : IMdLoader
//{
//    //private readonly IDataTypeKeys _dataTypeKeys;

//    //_dataTypeKeys = dataTypeKeys;

//    public OneOf<IQueryable<IDataType>, IEnumerable<Err>> Load()
//    {
//        var dataTpeKey = ECarcassDataTypeKeys.DataType.ToDtKey();
//        return OneOf<IQueryable<IDataType>, IEnumerable<Err>>.FromT0(
//            from mmj in ctx.ManyToManyJoins
//            join pt in ctx.DataTypes on mmj.PtId equals pt.DtId
//            join ct in ctx.DataTypes on mmj.CtId equals ct.DtId
//            join p in ctx.DataTypes on mmj.PKey equals p.DtKey
//            join c in ctx.DataTypes on mmj.CKey equals c.DtKey
//            where pt.DtKey == dataTpeKey && ct.DtKey == dataTpeKey
//            select new DataTypeToDataTypeModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.DtName + "." + c.DtName,
//                mmj.PKey));
//    }
//}

