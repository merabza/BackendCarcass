//using CarcassDb;
//using CarcassMasterDataDom.Models;
//using CarcassRepositories.MasterDataLoaders;
//using Microsoft.AspNetCore.Identity;

//namespace CarcassRepositories;

//public static class MdRepoCreator
//{
//    public static IMdCrudRepo CreateMdCruderRepo(CarcassDbContext ctx, string tableName,
//        RoleManager<AppRole> roleManager,
//        UserManager<AppUser> userManager)
//    {
//        return tableName switch
//        {
//            "roles" => new RolesMdRepo(roleManager),
//            "users" => new UsersMdRepo(userManager),
//            _ => new MdCrudRepoBase(ctx, tableName)
//        };
//    }

//    public static IMdLoader CreateMdLoaderRepo(CarcassDbContext ctx, string tableName, RoleManager<AppRole> roleManager,
//        UserManager<AppUser> userManager)
//    {
//        return tableName switch
//        {
//            "dataTypesToDataTypes" => new DataTypesToDataTypesMdLoader(ctx),
//            "dataTypesToCrudTypes" => new DataTypesToCrudTypesMdLoader(ctx),
//            _ => CreateMdCruderRepo(ctx, tableName, roleManager, userManager)
//        };
//    }
//}


