//using System.Linq;
//using System.Threading.Tasks;
//using CarcassMasterDataDom;
//using LanguageExt;
//using OneOf;
//using SystemToolsShared;

//namespace CarcassRepositories;

//public interface IMasterDataRepository
//{
//    OneOf<IQueryable<IDataType>, IEnumerable<Err>> EntitiesByTableName(string tableName);

//    Task<Option<IEnumerable<Err>>> DeleteEntityByTableNameAndKey(string tableName, int id);
//    Task<OneOf<IDataType, IEnumerable<Err>>> AddEntityByTableName(string tableName, string json);
//    Task<Option<IEnumerable<Err>>> UpdateEntityByTableName(string tableName, int id, string json);
//}

