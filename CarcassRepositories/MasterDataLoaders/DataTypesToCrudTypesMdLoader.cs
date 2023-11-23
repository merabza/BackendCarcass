//using System.Linq;
//using CarcassDb;
//using CarcassDb.QueryModels;
//using CarcassMasterDataDom;
//using OneOf;
//using SystemToolsShared;

//namespace CarcassRepositories.MasterDataLoaders;

//public sealed class DataTypesToCrudTypesMdLoader(CarcassDbContext ctx) : IMdLoader
//{
//    //private readonly IDataTypeKeys _dataTypeKeys;

//    //_dataTypeKeys = dataTypeKeys;

//    public OneOf<IQueryable<IDataType>, Err[]> Load()
//    {
//        return OneOf<IQueryable<IDataType>, Err[]>.FromT0(
//            from mmj in ctx.ManyToManyJoins
//            join pt in ctx.DataTypes on mmj.PtId equals pt.DtId
//            join ct in ctx.DataTypes on mmj.CtId equals ct.DtId
//            join p in ctx.DataTypes on mmj.PKey equals p.DtKey
//            join c in ctx.CrudRightTypes on mmj.CKey equals c.CrtKey
//            where pt.DtKey == ECarcassDataTypeKeys.DataType.ToDtKey() &&
//                  ct.DtKey == ECarcassDataTypeKeys.CrudRightType.ToDtKey()
//            select new DataTypeToCrudTypeModel(mmj.MmjId, mmj.PKey + "." + mmj.CKey, p.DtName + "." + c.CrtName,
//                p.DtId));
//    }
//}