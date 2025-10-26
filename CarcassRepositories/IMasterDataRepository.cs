//using System.Linq;
//using System.Threading.Tasks;
//using CarcassMasterDataDom;
//using LanguageExt;
//using OneOf;
//using SystemToolsShared;

//namespace CarcassRepositories;

//public interface IMasterDataRepository
//{
//    OneOf<IQueryable<IDataType>, Err[]> EntitiesByTableName(string tableName);

//    Task<Option<Err[]>> DeleteEntityByTableNameAndKey(string tableName, int id);
//    Task<OneOf<IDataType, Err[]>> AddEntityByTableName(string tableName, string json);
//    Task<Option<Err[]>> UpdateEntityByTableName(string tableName, int id, string json);
//}

