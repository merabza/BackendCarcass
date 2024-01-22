using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassMasterDataDom.CellModels;
using CarcassMasterDataDom.Models;
using LanguageExt;
using LibCrud;
using LibCrud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OneOf;
using SystemToolsShared;

namespace CarcassMasterDataDom.Crud;

public class MasterDataCrud : CrudBase, IMasterDataLoader
{
    private readonly ICarcassMasterDataRepository _cmdRepo;
    private readonly string _tableName;
    private readonly IEntityType _entityType;
    private IDataType? _justCreated;

    public static OneOf<MasterDataCrud, Err[]> Create(string tableName, ILogger logger,
        ICarcassMasterDataRepository cmdRepo)
    {
        var entityType = cmdRepo.GetEntityTypeByTableName(tableName);
        if (entityType == null)
            return new[] { MasterDataApiErrors.TableNotFound(tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

        return new MasterDataCrud(tableName, entityType, logger, cmdRepo);
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    private MasterDataCrud(string tableName, IEntityType entityType, ILogger logger,
        ICarcassMasterDataRepository cmdRepo) : base(logger, cmdRepo)
    {
        _tableName = tableName;
        _entityType = entityType;
        _cmdRepo = cmdRepo;
    }

    protected override int JustCreatedId => _justCreated?.Id ?? 0;

    public async Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken)
    {
        //var queryResult = Query();
        //if (queryResult.IsT1)
        //    return queryResult.AsT1;

        //var res = await queryResult.AsT0.ToListAsync(cancellationToken);
        //return res; // OneOf<IEnumerable<IDataType>, Err[]>.FromT0(res);

        return await Query().Match<Task<OneOf<IEnumerable<IDataType>, Err[]>>>(
            async x => await x.ToListAsync(cancellationToken),
            e => Task.FromResult<OneOf<IEnumerable<IDataType>, Err[]>>(e));
    }

    public override async Task<OneOf<TableRowsData, Err[]>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken)
    {
        var queryResult = QueryObject();
        if (queryResult.IsT1)
            return queryResult.AsT1;

        var query = queryResult.AsT0;

        var method = typeof(MasterDataCrud).GetMethod(nameof(UseUseCustomSortFilterPagination), 1,
            [typeof(object), typeof(FilterSortRequest), typeof(CancellationToken)]);
        //var method = typeof(MasterDataCrud).GetMethod(nameof(UseUseCustomSortFilterPagination));
        var generic = method?.MakeGenericMethod(_entityType.ClrType);
        if (generic is null)
            return new[]
            {
                new Err { ErrorCode = "GenericMethodDoesNotCreated", ErrorMessage = "Generic Method Does Not Created" }
            };
        var result = (Task<TableRowsData>?)generic.Invoke(this, [query, filterSortRequest, cancellationToken]);
        if (result is null)
            return new[]
                { new Err { ErrorCode = "TaskMethodDoesNotCreated", ErrorMessage = "Task Method Does Not Created" } };
        return await result;

        //var (realOffset, count, rows) = await query.UseCustomSortFilterPagination(filterSortRequest,
        //            s => s.EditFields(), cancellationToken, _entityType.ClrType);
        //        return new TableRowsData(count, realOffset, rows);
    }

    public async Task<TableRowsData> UseUseCustomSortFilterPagination<T>(object query,
        FilterSortRequest filterSortRequest, CancellationToken cancellationToken) where T : class, IDataType
    {
        var tQuery = (IQueryable<T>)query;
        //tQuery.Include()
        if (filterSortRequest.SortByFields?.Length > 0)
        {
            var gridModel = await _cmdRepo.GetDataTypeGridRulesByTableName(_tableName, cancellationToken);
            //DtNameFieldName
            if (gridModel is not null)
            {
                foreach (var sortField in filterSortRequest.SortByFields)
                {
                    var cell = gridModel.Cells.SingleOrDefault(x => x.FieldName == sortField.FieldName);

                    if (cell is null)
                        continue;
                    if (cell.TypeName != "MdLookup")
                        continue;
                    if (cell is not MdLookupCell { DtTable: not null } mdLookupCell)
                        continue;
                    var sortFieldName =
                        await _cmdRepo.GetSortFieldNameByTableName(mdLookupCell.DtTable, cancellationToken);
                    if (sortFieldName is null)
                        continue;
                    tQuery.Include(mdLookupCell.DtTable);
                    sortField.FieldName = sortFieldName;
                    //sortField.PropObjType = Type.GetType($"{nameof(GrammarGeDb)}.{nameof(Models)}.{mdLookupCell.DtTable}");
                    //sortField.PropObjType = Type.GetType($"GrammarGeDb.Models.{mdLookupCell.DtTable}");
                    sortField.PropObjType = _cmdRepo.GetEntityTypeByTableName(mdLookupCell.DtTable)?.ClrType;
                }
            }
        }

        var (realOffset, count, rows) = await tQuery.UseCustomSortFilterPagination(filterSortRequest,
            s => s.EditFields(), cancellationToken);
        return new TableRowsData(count, realOffset, rows);
    }


    protected override async Task<OneOf<ICrudData, Err[]>> GetOneData(int id, CancellationToken cancellationToken)
    {
        var result = await GetOneRecord(id, cancellationToken);
        return result.Match<OneOf<ICrudData, Err[]>>(t0 => new MasterDataCrudLoadedData(t0.EditFields()),
            t1 => t1);
    }


    private async Task<OneOf<IDataType, Err[]>> GetOneRecord(int id, CancellationToken cancellationToken)
    {
        var errors = new List<Err>();
        var entResult = Query();
        if (entResult.IsT1)
            errors.AddRange(entResult.AsT1);
        var res = entResult.AsT0;

        var keyResult = GetSingleKeyName();
        if (keyResult.IsT1)
            errors.AddRange(keyResult.AsT1);
        var keyRes = keyResult.AsT0;

        //var enm = await res.ToListAsync(cancellationToken);
        //var idt = enm.SingleOrDefault( w => w.Id == id);
        var idt = await res.Where($"{keyRes} = {id}").SingleOrDefaultAsync(cancellationToken);
        if (idt is not null)
            return OneOf<IDataType, Err[]>.FromT0(idt);
        errors.Add(MasterDataApiErrors.EntryNotFound);
        return errors.ToArray();
    }

    private OneOf<string, Err[]> GetSingleKeyName()
    {
        var singleKey = _entityType.GetKeys().SingleOrDefault();
        if (singleKey == null)
            return new[] { MasterDataApiErrors.TableHaveNotSingleKey(_tableName) }; //ვერ ვიპოვეთ ერთადერთი გასაღები

        if (singleKey.Properties.Count != 1)
            return new[]
                { MasterDataApiErrors.TableSingleKeyMustHaveOneProperty(_tableName) }; //ვერ ვიპოვეთ ერთადერთი გასაღები

        return singleKey.Properties[0].Name;
    }

    private OneOf<object, Err[]> QueryObject()
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //


        //return _cmdRepo.LoadByTableName(_tableName);

        var setMethod = _cmdRepo.SetMethodInfo();
        if (setMethod == null)
            return new[] { MasterDataApiErrors.SetMethodNotFoundForTable(_tableName) }; //ცხრილს არ აქვს მეთოდი Set

        var result = _cmdRepo.RunGenericMethodForLoadAllRecords(setMethod, _entityType);
        return result == null
            ? new[]
            {
                MasterDataApiErrors.SetMethodReturnsNullForTable(_tableName)
            } //ცხრილის Set მეთოდი აბრუნებს null-ს
            : OneOf<object, Err[]>.FromT0(result);
    }


    private OneOf<IQueryable<IDataType>, Err[]> Query()
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //


        //return _cmdRepo.LoadByTableName(_tableName);

        var setMethod = _cmdRepo.SetMethodInfo();
        if (setMethod == null)
            return new[] { MasterDataApiErrors.SetMethodNotFoundForTable(_tableName) }; //ცხრილს არ აქვს მეთოდი Set

        var result = _cmdRepo.RunGenericMethodForLoadAllRecords(setMethod, _entityType);
        return result == null
            ? new[]
            {
                MasterDataApiErrors.SetMethodReturnsNullForTable(_tableName)
            } //ცხრილის Set მეთოდი აბრუნებს null-ს
            : OneOf<IQueryable<IDataType>, Err[]>.FromT0((IQueryable<IDataType>)result);
    }

    protected override async Task<Option<Err[]>> CreateData(ICrudData crudDataForCreate,
        CancellationToken cancellationToken)
    {
        var masterDataCrudDataForCreate = (MasterDataCrudData)crudDataForCreate;

        dynamic? jObj = JsonConvert.DeserializeObject(masterDataCrudDataForCreate.Json, _entityType.ClrType);
        if (jObj is not IDataType newItem)
            return new[]
            {
                MasterDataApiErrors.RecordDoesNotDeserialized(_tableName)
            }; //დესერიალიზაციისას არ მივიღეთ იმ ტიპის ობიექტი, რაც საჭირო იყო
        newItem.Id = 0;

        var validateResult = await Validate(newItem, cancellationToken);
        if (validateResult.IsSome)
            return (Err[])validateResult;

        await _cmdRepo.Create(newItem, cancellationToken);
        _justCreated = newItem;
        return null;
        //return createResult.Match(x => x, () => OneOf<IDataType, Err[]>.FromT0(newItem));
    }

    protected override async Task<Option<Err[]>> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken)
    {
        var masterDataCrudDataForUpdate = (MasterDataCrudData)crudDataNewVersion;

        dynamic? jObj = JsonConvert.DeserializeObject(masterDataCrudDataForUpdate.Json, _entityType.ClrType);
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

        var validateResult = await Validate(newItem, cancellationToken);
        if (validateResult.IsSome)
            return validateResult;

        //_cmdRepo.Update(newItem);

        //var crudMdRepo = MdRepoCreator.CreateMdCruderRepo(_context, _tableName, _roleManager, _userManager);
        return await Update(id, newItem, cancellationToken);
    }

    private async Task<Option<Err[]>> Update(int id, IDataType newItem, CancellationToken cancellationToken)
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //
        //if (idt == null)
        //    return new[]
        //    {
        //        MasterDataApiErrors.RecordNotFound(_tableName, id)
        //    }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound


        var result = await GetOneRecord(id, cancellationToken);
        return result.Match<Option<Err[]>>(r =>
        {
            r.UpdateTo(newItem);
            _cmdRepo.Update(r);
            return null;
        }, e => e);
    }

    protected override async Task<Option<Err[]>> DeleteData(int id, CancellationToken cancellationToken)
    {
        var result = await GetOneRecord(id, cancellationToken);
        return result.Match<Option<Err[]>>(r =>
        {
            _cmdRepo.Delete(r);
            return null;
        }, e => e);
    }

    private async Task<Option<Err[]>> Validate(IDataType newItem, CancellationToken cancellationToken)
    {
        //var dt = _context.DataTypes.SingleOrDefault(s => s.DtTable == tableName);
        var gridModel = await _cmdRepo.GetDataTypeGridRulesByTableName(_tableName, cancellationToken);

        if (gridModel == null)
            return new[] { MasterDataApiErrors.MasterDataInvalidValidationRules(_tableName) };

        List<Err> errors = [];
        var props = newItem.GetType().GetProperties();

        foreach (var cell in gridModel.Cells)
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