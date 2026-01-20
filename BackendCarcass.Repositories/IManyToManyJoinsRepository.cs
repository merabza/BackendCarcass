//using System;
//using System.Linq;
//using CarcassMasterDataDom;

//namespace CarcassRepositories;

//public interface IManyToManyJoinsRepository
//{
//    bool CheckUserToUserRight(string userName1, string userName2);

//    bool CheckUserAppClaimRight(string userName, string appClaimName);

//    int GetDataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey);

//    string? GetDataTypeKeyById(int dtId);

//    string? GetKeyByTableName(string tableName);

//    IQueryable<string> GetManyToManyJoinsPc(int parentTypeId, string parentKey, int childTypeId);

//    IQueryable<string> GetManyToManyJoinsCp(int childTypeId, string childKey, int parentTypeId);

//    IQueryable<string> GetManyToManyJoinsPcc(int parentTypeId, string parentKey, int childTypeId, int childTypeId2);

//    IQueryable<string> GetManyToManyJoinsPccc(int parentTypeId, string parentKey, int childTypeId, int childTypeId2,
//        int childTypeId3);

//    IQueryable<string> GetManyToManyJoinsPcc2(int parentTypeId, string parentKey, int childTypeId, int mmjDataId,
//        int childTypeId2, int childTypeId3);

//    IQueryable<string> GetManyToManyJoinsPcc3(int parentTypeId, string parentKey, int childTypeId, int mmjDataId,
//        int childTypeId2, int childTypeId3);

//    IQueryable<Tuple<string, string>> GetManyToManyJoinsPcc4(int parentTypeId, string parentKey, int childTypeId,
//        int mmjDataId, int childTypeId2, int childTypeId3);
//}


