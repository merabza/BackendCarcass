//using CarcassDb;
//using CarcassIdentity;
//using Microsoft.AspNetCore.Identity;

//namespace CarcassRepositories;

//public /*open*/ class CarcassMasterDataRepositoryOld : ManyToManyJoinsRepository//, IMasterDataRepository
//{
//    private readonly CarcassDbContext _context;
//    private readonly RoleManager<AppRole> _roleManager;
//    private readonly UserManager<AppUser> _userManager;

//    protected CarcassMasterDataRepositoryOld(CarcassDbContext context, RoleManager<AppRole> roleManager,
//        UserManager<AppUser> userManager) : base(context)
//    {
//        _context = context;
//        _roleManager = roleManager;
//        _userManager = userManager;
//    }

//    //public OneOf<IQueryable<IDataType>, Err[]> EntitiesByTableName(string tableName)
//    //{
//    //    var loaderMdRepo = CreateMdLoaderRepo(tableName);
//    //    return loaderMdRepo.Load();
//    //}

//    //public async Task<Option<Err[]>> DeleteEntityByTableNameAndKey(string tableName, int id)
//    //{
//    //    var crudMdRepo = MdRepoCreator.CreateMdCruderRepo(_context, tableName, _roleManager, _userManager);
//    //    return await crudMdRepo.Delete(id);
//    //}

//    //public async Task<Option<Err[]>> UpdateEntityByTableName(string tableName, int id, string json)
//    //{
//    //    var vvv = _context.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
//    //    if (vvv == null)
//    //        return new[] { MasterDataApiErrors.TableNotFound(tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

//    //    dynamic? jObj = JsonConvert.DeserializeObject(json, vvv.ClrType);
//    //    if (jObj is not IDataType newItem)
//    //        return new[]
//    //        {
//    //            MasterDataApiErrors.RecordDoesNotDeserialized(tableName)
//    //        }; //დესერიალიზაციისას არ მივიღეთ იმ ტიპის ობიექტი, რაც საჭირო იყო

//    //    if (newItem.Id != id)
//    //        return
//    //            new[]
//    //            {
//    //                MasterDataApiErrors.WrongId(tableName)
//    //            }; //მოწოდებული ინფორმაცია არასწორია, რადგან იდენტიფიკატორი არ ემთხვევა მოწოდებული ობიექტის იდენტიფიკატორს

//    //    var validateResult = Validate(newItem, tableName);
//    //    if (validateResult.IsSome)
//    //        return validateResult;

//    //    var crudMdRepo = MdRepoCreator.CreateMdCruderRepo(_context, tableName, _roleManager, _userManager);
//    //    return await crudMdRepo.Update(id, newItem);
//    //}

//    //public bool CheckUserRightToClaim(IEnumerable<Claim> userClaims, string claimName)
//    //{
//    //    return GetRoles(userClaims).Any(roleName => CheckRoleRightToClaim(roleName, claimName));
//    //}

//    //private bool CheckRoleRightToClaim(string roleName, string claimName)
//    //{
//    //    var roleDtId = GetDataTypeIdByKey(ECarcassDataTypeKeys.Role);
//    //    var appClaimDataTypeId = GetDataTypeIdByKey(ECarcassDataTypeKeys.AppClaim);

//    //    return _context.ManyToManyJoins.Any(w =>
//    //        w.PtId == roleDtId && w.PKey == roleName && w.CtId == appClaimDataTypeId && w.CKey == claimName);
//    //}

//    //private static IEnumerable<string> GetRoles(IEnumerable<Claim> claims)
//    //{
//    //    return claims.Where(so => so.Type == ClaimTypes.Role).Select(claim => claim.Value);
//    //}

//    //private bool CheckMenuRight(string roleName, string menuItemName)
//    //{
//    //    //CarcassContext.ManyToManyJoins.

//    //    int menuGroupsDtId = GetDataTypeIdByKey(ECarcassDataTypeKeys.MenuGroup);
//    //    int menuDtId = GetDataTypeIdByKey(ECarcassDataTypeKeys.MenuItm);
//    //    int roleDtId = GetDataTypeIdByKey(ECarcassDataTypeKeys.Role);

//    //    IQueryable<ManyToManyJoin> res = from m in _context.Menu
//    //        join mg in _context.MenuGroups on m.MenGroupId equals mg.MengId
//    //        join rm in _context.ManyToManyJoins on m.MenKey equals rm.CKey
//    //        join rmg in _context.ManyToManyJoins on mg.MengKey equals rmg.CKey
//    //        where rm.PtId == roleDtId && rm.PKey == roleName && rm.CtId == menuDtId && rm.CKey == menuItemName &&
//    //              rmg.PtId == roleDtId && rmg.PKey == roleName && rmg.CtId == menuGroupsDtId
//    //        select rm;

//    //    return res.Any();

//    //}

//    //public async Task<OneOf<IDataType, Err[]>> AddEntityByTableName(string tableName, string json)
//    //{
//    //    var vvv = _context.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
//    //    if (vvv == null)
//    //        return new[] { MasterDataApiErrors.TableNotFound(tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

//    //    dynamic? jObj = JsonConvert.DeserializeObject(json, vvv.ClrType);
//    //    if (jObj is not IDataType newItem)
//    //        return new[]
//    //        {
//    //            MasterDataApiErrors.RecordDoesNotDeserialized(tableName)
//    //        }; //დესერიალიზაციისას არ მივიღეთ იმ ტიპის ობიექტი, რაც საჭირო იყო
//    //    newItem.Id = 0;

//    //    var validateResult = Validate(newItem, tableName);
//    //    if (validateResult.IsSome)
//    //        return (Err[])validateResult;

//    //    var crudMdRepo = MdRepoCreator.CreateMdCruderRepo(_context, tableName, _roleManager, _userManager);
//    //    var createResult = await crudMdRepo.Create(newItem);
//    //    return createResult.Match(x => x, () => OneOf<IDataType, Err[]>.FromT0(newItem));
//    //}

//    //protected virtual IMdLoader CreateMdLoaderRepo(string tableName)
//    //{
//    //    return MdRepoCreator.CreateMdLoaderRepo(_context, tableName, _roleManager, _userManager);
//    //}

//    //private Option<Err[]> Validate(IDataType newItem, string tableName)
//    //{
//    //    var dt = _context.DataTypes.SingleOrDefault(s => s.DtTable == tableName);

//    //    if (dt == null)
//    //        return new[] { MasterDataApiErrors.MasterDataTableNotFound(tableName) };

//    //    if (dt.DtGridRulesJson == null)
//    //        return null;

//    //    var gm = GridModel.DeserializeGridModel(dt.DtGridRulesJson);
//    //    if (gm == null)
//    //        return new[] { MasterDataApiErrors.MasterDataInvalidValidationRules(tableName) };

//    //    List<Err> errors = new();
//    //    var props = newItem.GetType().GetProperties();

//    //    foreach (var cell in gm.Cells)
//    //    {
//    //        var prop = props.SingleOrDefault(w => w.Name == cell.FieldName.CapitalizeCamel());
//    //        if (prop == null)
//    //        {
//    //            errors.Add(MasterDataApiErrors.MasterDataFieldNotFound(tableName, cell.FieldName));
//    //            continue;
//    //        }

//    //        var mes = cell.Validate(prop.GetValue(newItem));
//    //        if (mes.Count > 0)
//    //            errors.AddRange(mes);
//    //    }

//    //    return errors.Count == 0 ? null : errors.ToArray();
//    //}
//}


