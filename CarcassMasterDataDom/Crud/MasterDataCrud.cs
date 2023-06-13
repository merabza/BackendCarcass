using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassMasterDataDom.Models;
using LanguageExt;
using LibCrud;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom.Crud;

public class MasterDataCrud : CrudBase, IMasterDataLoader
{
    private readonly ICarcassMasterDataRepository _cmdRepo;
    private readonly string _tableName;

    public MasterDataCrud(string tableName, ILogger logger, ICarcassMasterDataRepository cmdRepo) : base(logger,
        cmdRepo)
    {
        _tableName = tableName;
        _cmdRepo = cmdRepo;
    }

    public async Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords()
    {
        var queryResult = Query();
        if (queryResult.IsT1)
            return queryResult.AsT1;

        var res = await queryResult.AsT0.ToListAsync();
        return res; // OneOf<IEnumerable<IDataType>, Err[]>.FromT0(res);
    }

    protected override async Task<OneOf<ICrudData, Err[]>> GetOneData(int id)
    {
        var result = await GetOneRecord(id);
        return result.Match<OneOf<ICrudData, Err[]>>(t0 => new MasterDataCrudLoadedData(t0.EditFields()),
            t1 => t1);
    }


    private async Task<OneOf<IDataType, Err[]>> GetOneRecord(int id)
    {
        var errors = new List<Err>();
        var entResult = Query();
        if (entResult.IsT1)
            errors.AddRange(entResult.AsT1);
        var res = entResult.AsT0;
        var idt = await res.SingleOrDefaultAsync(w => w.Id == id);
        if (idt is not null)
            return OneOf<IDataType, Err[]>.FromT0(idt);
        errors.Add(MasterDataApiErrors.EntryNotFound);
        return errors.ToArray();
    }

    private OneOf<IQueryable<IDataType>, Err[]> Query()
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //


        //return _cmdRepo.LoadByTableName(_tableName);
        var entityType = _cmdRepo.GetEntityTypeByTableName(_tableName);
        if (entityType == null)
            return new[] { MasterDataApiErrors.TableNotFound(_tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

        var setMethod = _cmdRepo.MethodInfo();
        if (setMethod == null)
            return new[] { MasterDataApiErrors.SetMethodNotFoundForTable(_tableName) }; //ცხრილს არ აქვს მეთოდი Set

        var result = _cmdRepo.RunGenericMethodForLoadAllRecords(setMethod, entityType);
        return result == null
            ? new[]
            {
                MasterDataApiErrors.SetMethodReturnsNullForTable(_tableName)
            } //ცხრილის Set მეთოდი აბრუნებს null-ს
            : OneOf<IQueryable<IDataType>, Err[]>.FromT0((IQueryable<IDataType>)result);
    }

    protected override async Task<Option<Err[]>> CreateData(ICrudData crudDataForCreate)
    {
        var masterDataCrudDataForCreate = (MasterDataCrudData)crudDataForCreate;
        var entityType = _cmdRepo.GetEntityTypeByTableName(_tableName);
        if (entityType == null)
            return new[] { MasterDataApiErrors.TableNotFound(_tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

        dynamic? jObj = JsonConvert.DeserializeObject(masterDataCrudDataForCreate.Json, entityType.ClrType);
        if (jObj is not IDataType newItem)
            return new[]
            {
                MasterDataApiErrors.RecordDoesNotDeserialized(_tableName)
            }; //დესერიალიზაციისას არ მივიღეთ იმ ტიპის ობიექტი, რაც საჭირო იყო
        newItem.Id = 0;

        var validateResult = Validate(newItem);
        if (validateResult.IsSome)
            return (Err[])validateResult;

        await _cmdRepo.Create(newItem);
        JustCreatedId = newItem.Id;
        return null;
        //return createResult.Match(x => x, () => OneOf<IDataType, Err[]>.FromT0(newItem));
    }

    protected override async Task<Option<Err[]>> UpdateData(int id, ICrudData crudDataNewVersion)
    {
        var masterDataCrudDataForUpdate = (MasterDataCrudData)crudDataNewVersion;
        var entityType = _cmdRepo.GetEntityTypeByTableName(_tableName);
        if (entityType == null)
            return new[] { MasterDataApiErrors.TableNotFound(_tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

        dynamic? jObj = JsonConvert.DeserializeObject(masterDataCrudDataForUpdate.Json, entityType.ClrType);
        if (jObj is not IDataType newItem)
            return new[]
            {
                MasterDataApiErrors.RecordDoesNotDeserialized(_tableName)
            }; //დესერიალიზაციისას არ მივიღეთ იმ ტიპის ობიექტი, რაც საჭირო იყო

        if (newItem.Id != id)
            return
                new[]
                {
                    MasterDataApiErrors.WrongId(_tableName)
                }; //მოწოდებული ინფორმაცია არასწორია, რადგან იდენტიფიკატორი არ ემთხვევა მოწოდებული ობიექტის იდენტიფიკატორს

        var validateResult = Validate(newItem);
        if (validateResult.IsSome)
            return validateResult;

        //_cmdRepo.Update(newItem);

        //var crudMdRepo = MdRepoCreator.CreateMdCruderRepo(_context, _tableName, _roleManager, _userManager);
        return await Update(id, newItem);
    }

    private async Task<Option<Err[]>> Update(int id, IDataType newItem)
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //
        //if (idt == null)
        //    return new[]
        //    {
        //        MasterDataApiErrors.RecordNotFound(_tableName, id)
        //    }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound


        var result = await GetOneRecord(id);
        return result.Match<Option<Err[]>>(r =>
        {
            r.UpdateTo(newItem);
            _cmdRepo.Update(r);
            return null;
        }, e => e);
    }

    protected override async Task<Option<Err[]>> DeleteData(int id)
    {
        var result = await GetOneRecord(id);
        return result.Match<Option<Err[]>>(r =>
        {
            _cmdRepo.Delete(r);
            return null;
        }, e => e);
    }

    private Option<Err[]> Validate(IDataType newItem)
    {
        //var dt = _context.DataTypes.SingleOrDefault(s => s.DtTable == tableName);
        var dtGridRulesJson = _cmdRepo.GetDataTypeGridRulesByTableName(_tableName);

        //if (dt == null)
        //    return new[] { MasterDataApiErrors.MasterDataTableNotFound(_tableName) };

        if (dtGridRulesJson == null)
            return null;

        var gm = GridModel.DeserializeGridModel(dtGridRulesJson);
        if (gm == null)
            return new[] { MasterDataApiErrors.MasterDataInvalidValidationRules(_tableName) };

        List<Err> errors = new();
        var props = newItem.GetType().GetProperties();

        foreach (var cell in gm.Cells)
        {
            var prop = props.SingleOrDefault(w => w.Name == cell.FieldName.CapitalizeCamel());
            if (prop == null)
            {
                errors.Add(MasterDataApiErrors.MasterDataFieldNotFound(_tableName, cell.FieldName));
                continue;
            }

            var mes = cell.Validate(prop.GetValue(newItem));
            if (mes.Count > 0)
                errors.AddRange(mes);
        }

        return errors.Count == 0 ? null : errors.ToArray();
    }
}