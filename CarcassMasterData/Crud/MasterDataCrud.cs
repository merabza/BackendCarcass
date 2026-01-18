using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassMasterData.CellModels;
using CarcassMasterData.Models;
using CarcassMasterData.SortIdStuff;
using DomainShared.Repositories;
using LanguageExt;
using LibCrud;
using LibCrud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassMasterData.Crud;

public sealed class MasterDataCrud : CrudBase, IMasterDataLoader
{
    private readonly ICarcassMasterDataRepository _cmdRepo;
    private readonly IEntityType _entityType;
    private readonly string _tableName;
    private GridModel? _gridModel;
    private IDataType? _justCreated;
    private ISortIdHelper? _sortHelper;

    // ReSharper disable once ConvertToPrimaryConstructor
    private MasterDataCrud(string tableName, IEntityType entityType, ILogger logger,
        ICarcassMasterDataRepository cmdRepo, IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _tableName = tableName;
        _entityType = entityType;
        _cmdRepo = cmdRepo;
    }

    protected override int JustCreatedId => _justCreated?.Id ?? 0;

    public async ValueTask<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        var queryResult = Query();
        if (queryResult.IsT1)
        {
            return queryResult.AsT1;
        }

        var isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsT1)
        {
            return isGridWithSortIdResult.AsT1;
        }

        bool isGridWithSortId = isGridWithSortIdResult.AsT0;

        var query = queryResult.AsT0;

        if (!isGridWithSortId)
        {
            return await query.ToListAsync(cancellationToken);
        }

        var method = typeof(MasterDataCrud).GetMethod(nameof(OrderBySortId), 1, [typeof(object)]);
        var generic = method?.MakeGenericMethod(_entityType.ClrType);
        if (generic is null)
        {
            return new[] { MasterDataCrudErrors.GenericMethodWasNotCreated(nameof(OrderBySortId)) };
        }

        object? queryRunResult = generic.Invoke(this, [query]);
        if (queryRunResult is null)
        {
            return new[] { MasterDataCrudErrors.MethodResultIsNull(nameof(OrderBySortId)) };
        }

        return (List<IDataType>)queryRunResult;

        //return await Query().Match<Task<OneOf<IEnumerable<IDataType>, Err[]>>>(
        //    async x => await x.ToListAsync(cancellationToken),
        //    e => Task.FromResult<OneOf<IEnumerable<IDataType>, Err[]>>(e));
    }

    public static OneOf<MasterDataCrud, Err[]> Create(string tableName, ILogger logger,
        ICarcassMasterDataRepository cmdRepo, IUnitOfWork unitOfWork)
    {
        var entityType = cmdRepo.GetEntityTypeByTableName(tableName);
        if (entityType is null)
        {
            return new[] { MasterDataApiErrors.TableNotFound(tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი
        }

        return new MasterDataCrud(tableName, entityType, logger, cmdRepo, unitOfWork);
    }

    private async Task<OneOf<bool, Err[]>> IsGridWithSortId(CancellationToken cancellationToken = default)
    {
        var gridModel = await GetDataTypeGridRulesByTableName(cancellationToken);
        if (gridModel is null)
        {
            return new[] { MasterDataCrudErrors.GridModelIsNull(_tableName) };
        }

        IntegerCell? sortIdCell = null;
        foreach (var cell in gridModel.Cells.Where(x => x.TypeName == "Integer"))
        {
            if (cell is not IntegerCell intCell)
            {
                continue;
            }

            if (!intCell.IsSortId)
            {
                continue;
            }

            sortIdCell = intCell;
            break;
        }

        return sortIdCell is not null;
    }

    public static IEnumerable<IDataType> OrderBySortId<T>(object query) where T : class, ISortedDataType
    {
        var tQuery = (IQueryable<T>)query;
        return tQuery.OrderBy(x => x.SortId).Select(delegate(T s)
        {
            //s.SortId++;
            return (IDataType)s;
        }).ToList();
    }

    public override async ValueTask<OneOf<TableRowsData, Err[]>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken = default)
    {
        var queryResult = QueryObject();
        if (queryResult.IsT1)
        {
            return queryResult.AsT1;
        }

        object? query = queryResult.AsT0;

        var method = typeof(MasterDataCrud).GetMethod(nameof(UseCustomSortFilterPagination), 1,
            [typeof(object), typeof(FilterSortRequest), typeof(CancellationToken)]);
        //var method = typeof(MasterDataCrud).GetMethod(nameof(UseUseCustomSortFilterPagination));
        var generic = method?.MakeGenericMethod(_entityType.ClrType);
        if (generic is null)
        {
            return new[] { MasterDataCrudErrors.GenericMethodWasNotCreated(nameof(UseCustomSortFilterPagination)) };
        }

        // ReSharper disable once using
        using var result = (Task<TableRowsData>?)generic.Invoke(this, [query, filterSortRequest, cancellationToken]);
        if (result is null)
        {
            return new[] { MasterDataCrudErrors.MethodResultTaskIsNull(nameof(UseCustomSortFilterPagination)) };
        }

        return await result;

        //var (realOffset, count, rows) = await query.UseCustomSortFilterPagination(filterSortRequest,
        //            s => s.EditFields(), cancellationToken, _entityType.ClrType);
        //        return new TableRowsData(count, realOffset, rows);
    }

    private async ValueTask<GridModel?> GetDataTypeGridRulesByTableName(CancellationToken cancellationToken = default)
    {
        return _gridModel ??= await _cmdRepo.GetDataTypeGridRulesByTableName(_tableName, cancellationToken);
    }

    public async Task<TableRowsData> UseCustomSortFilterPagination<T>(object query, FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken = default) where T : class, IDataType
    {
        var tQuery = (IQueryable<T>)query;
        //tQuery.Include()
        if (filterSortRequest.SortByFields?.Length > 0)
        {
            var gridModel = await GetDataTypeGridRulesByTableName(cancellationToken);
            //DtNameFieldName
            if (gridModel is not null)
            {
                foreach (var sortField in filterSortRequest.SortByFields)
                {
                    var cell = gridModel.Cells.SingleOrDefault(x => x.FieldName == sortField.FieldName);

                    if (cell is null)
                    {
                        continue;
                    }

                    if (cell.TypeName != "MdLookup")
                    {
                        continue;
                    }

                    if (cell is not MdLookupCell { DtTable: not null } mdLookupCell)
                    {
                        continue;
                    }

                    string? sortFieldName =
                        await _cmdRepo.GetSortFieldNameByTableName(mdLookupCell.DtTable, cancellationToken);
                    if (sortFieldName is null)
                    {
                        continue;
                    }

                    tQuery.Include(mdLookupCell.DtTable);
                    sortField.FieldName = sortFieldName;
                    sortField.PropObjType = _cmdRepo.GetEntityTypeByTableName(mdLookupCell.DtTable)?.ClrType;
                }
            }
        }

        var (realOffset, count, rows) = await tQuery.UseCustomSortFilterPagination(filterSortRequest,
            s => s.EditFields(), cancellationToken);
        return new TableRowsData(count, realOffset, rows);
    }

    protected override async Task<OneOf<ICrudData, Err[]>> GetOneData(int id,
        CancellationToken cancellationToken = default)
    {
        var getOneRecordResult = await GetOneRecord(id, cancellationToken);
        if (getOneRecordResult.IsT1)
        {
            return getOneRecordResult.AsT1;
        }

        var isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsT1)
        {
            return isGridWithSortIdResult.AsT1;
        }

        bool isGridWithSortId = isGridWithSortIdResult.AsT0;

        if (!isGridWithSortId)
        {
            return new MasterDataCrudLoadedData(getOneRecordResult.AsT0.EditFields());
        }

        var sortedData = (ISortedDataType)getOneRecordResult.AsT0;
        sortedData.SortId++;
        return new MasterDataCrudLoadedData(sortedData.EditFields());

        //return getOneRecordResult.Match<OneOf<ICrudData, Err[]>>(t0 => new MasterDataCrudLoadedData(t0.EditFields()),
        //    t1 => t1);
    }

    private async Task<OneOf<IDataType, Err[]>> GetOneRecord(int id, CancellationToken cancellationToken = default)
    {
        var errors = new List<Err>();
        var entResult = Query();
        if (entResult.IsT1)
        {
            errors.AddRange(entResult.AsT1);
        }

        var res = entResult.AsT0;

        var keyResult = GetSingleKeyPropertyName();
        if (keyResult.IsT1)
        {
            errors.AddRange(keyResult.AsT1);
        }

        string? keyPropertyName = keyResult.AsT0;

        var parameter = Expression.Parameter(_entityType.ClrType, keyPropertyName);
        var constant = Expression.Constant(id);
        var equal = Expression.Equal(parameter, constant);
        var lambda = Expression.Lambda<Func<IDataType, bool>>(equal, parameter);
        var idt = await res.Where(lambda).SingleOrDefaultAsync(cancellationToken);

        if (idt is not null)
        {
            return OneOf<IDataType, Err[]>.FromT0(idt);
        }

        errors.Add(MasterDataApiErrors.EntryNotFound());
        return errors.ToArray();
    }

    private OneOf<string, Err[]> GetSingleKeyPropertyName()
    {
        var singleKey = _entityType.GetKeys().SingleOrDefault();
        if (singleKey is null)
        {
            return new[] { MasterDataApiErrors.TableHaveNotSingleKey(_tableName) }; //ვერ ვიპოვეთ ერთადერთი გასაღები
        }

        if (singleKey.Properties.Count != 1)
        {
            return new[]
            {
                MasterDataApiErrors.TableSingleKeyMustHaveOneProperty(_tableName)
            }; //ვერ ვიპოვეთ ერთადერთი გასაღები
        }

        return OneOf<string, Err[]>.FromT0(singleKey.Properties[0].Name);
        //return OneOf<IProperty, Err[]>.FromT0(singleKey.Properties[0]);
    }

    private OneOf<object, Err[]> QueryObject()
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //

        //return _cmdRepo.LoadByTableName(_tableName);

        var setMethod = _cmdRepo.SetMethodInfo();
        if (setMethod is null)
        {
            return new[] { MasterDataApiErrors.SetMethodNotFoundForTable(_tableName) }; //ცხრილს არ აქვს მეთოდი Set
        }

        object? result = _cmdRepo.RunGenericMethodForLoadAllRecords(setMethod, _entityType);
        return result is null
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
        if (setMethod is null)
        {
            return new[] { MasterDataApiErrors.SetMethodNotFoundForTable(_tableName) }; //ცხრილს არ აქვს მეთოდი Set
        }

        object? result = _cmdRepo.RunGenericMethodForLoadAllRecords(setMethod, _entityType);
        return result is null
            ? new[]
            {
                MasterDataApiErrors.SetMethodReturnsNullForTable(_tableName)
            } //ცხრილის Set მეთოდი აბრუნებს null-ს
            : OneOf<IQueryable<IDataType>, Err[]>.FromT0((IQueryable<IDataType>)result);
    }

    protected override async ValueTask<Option<Err[]>> CreateData(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default)
    {
        var masterDataCrudDataForCreate = (MasterDataCrudData)crudDataForCreate;

        dynamic? jObj = JsonConvert.DeserializeObject(masterDataCrudDataForCreate.Json, _entityType.ClrType);
        if (jObj is not IDataType newItem)
        {
            return new[]
            {
                MasterDataApiErrors.RecordDoesNotDeserialized(_tableName)
            }; //დესერიალიზაციისას არ მივიღეთ იმ ტიპის ობიექტი, რაც საჭირო იყო
        }

        newItem.Id = 0;

        var validateResult = await Validate(newItem, cancellationToken);
        if (validateResult.IsSome)
        {
            return (Err[])validateResult;
        }

        var isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsT1)
        {
            return isGridWithSortIdResult.AsT1;
        }

        bool isGridWithSortId = isGridWithSortIdResult.AsT0;

        if (!isGridWithSortId)
        {
            await _cmdRepo.Create(newItem, cancellationToken);
            _justCreated = newItem;
            return null;
        }

        //უნდა მოხდეს SortId-ის დამუშავება შემდეგნაირად:
        //1. თუ SortId <= 0-ზე,
        //1.1. უნდა მოხდეს არსებული SortId-ების მაქსიმუმის დათვლა
        //1.2. მიღებულ მაქსიმუმს დაემატოს 1
        //1.3. მიღებული რიცხვით ჩანაცვლდეს SortId-ის მნიშვნელობა
        //1.4. მოხდეს ახალი ჩანაწერის შენახვა

        var sortIdHelperType = typeof(SortIdHelper<>).MakeGenericType(_entityType.ClrType);
        if (Activator.CreateInstance(sortIdHelperType, _cmdRepo) is not ISortIdHelper sortHelper)
        {
            return new[] { MasterDataCrudErrors.SortIdHelperWasNotCreatedForType(_entityType.ClrType) };
        }

        var queryResult = Query();
        if (queryResult.IsT1)
        {
            return queryResult.AsT1;
        }

        //მაქსიმუმის დათვლა სხვადასხვა მიზეზებით გვჭირდება, ამიტომ ვითვლით აქ.
        int sortIdMax = sortHelper.CountSortIdMax(queryResult.AsT0);

        var newItemWsi = (ISortedDataType)newItem;
        if (newItemWsi.SortId <= 0)
        {
            newItemWsi.SortId = sortIdMax + 1;
        }

        //2. თუ SortId > 0-ზე,
        //2.1. SortId--
        //2.2. ვიპოვოთ SortId-ის შესაბამისი ჩანაწერი არსებობს თუ არა ცხრილში
        //2.2.2. თუ არსებობს
        //2.2.2.1 ყველა ჩანაწერი, რომლი SortId >= შესანახ SortId-ს, ყველას გავუზარდოთ 1-ით
        //2.2.3 ვამატებთ ახალ ჩანაწერს არსებული SortId მნიშვნელობით
        else
        {
            newItemWsi.SortId--;
            if (await sortHelper.IsSortIdExists(queryResult.AsT0, newItemWsi.SortId, 0))
            {
                await sortHelper.IncreaseSortIds(queryResult.AsT0, newItemWsi.SortId, sortIdMax, 0, cancellationToken);
            }
        }
        //3. დავადგინოთ არის თუ არა ისეთი ჩანაწერები, რომლებიც იწვევს SortId-ის ჩავარდნას და გამოვასწოროთ ჩავარდნები.
        //3.1 უნდა ჩავტვირთოთ იდენტიფიკატორები, SortId-ები, RowId-ები დალაგებული SortId-ებით
        //3.2. ისეთი ჩანაწერებისათვის რომლებისთვისაც SortId != RowId, გავაახლოთ SortId, RowId-ის მნიშვნელობით.

        //await sortHelper.ReSortSortIds(queryResult.AsT0, cancellationToken);

        await _cmdRepo.Create(newItem, cancellationToken);
        _justCreated = newItem;
        return null;

        //return createResult.Match(x => x, () => OneOf<IDataType, Err[]>.FromT0(newItem));
    }

    protected override async ValueTask<Option<Err[]>> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default)
    {
        var masterDataCrudDataForUpdate = (MasterDataCrudData)crudDataNewVersion;

        dynamic? jObj = JsonConvert.DeserializeObject(masterDataCrudDataForUpdate.Json, _entityType.ClrType);
        if (jObj is not IDataType newItem)
        {
            return new[]
            {
                MasterDataApiErrors.RecordDoesNotDeserialized(_tableName)
            }; //დესერიალიზაციისას არ მივიღეთ იმ ტიპის ობიექტი, რაც საჭირო იყო
        }

        if (newItem.Id != id)
        {
            return
                new[]
                {
                    MasterDataApiErrors.WrongId(_tableName)
                }; //მოწოდებული ინფორმაცია არასწორია, რადგან იდენტიფიკატორი არ ემთხვევა მოწოდებული ობიექტის იდენტიფიკატორს
        }

        var validateResult = await Validate(newItem, cancellationToken);
        if (validateResult.IsSome)
        {
            return validateResult;
        }

        var isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsT1)
        {
            return isGridWithSortIdResult.AsT1;
        }

        bool isGridWithSortId = isGridWithSortIdResult.AsT0;

        if (!isGridWithSortId)
        {
            return await Update(id, newItem, cancellationToken);
        }

        //უნდა მოხდეს SortId-ის დამუშავება შემდეგნაირად:
        //1. თუ SortId <= 0-ზე,
        //1.1. უნდა მოხდეს არსებული SortId-ების მაქსიმუმის დათვლა
        //1.2. მიღებულ მაქსიმუმს დაემატოს 1
        //1.3. მიღებული რიცხვით ჩანაცვლდეს SortId-ის მნიშვნელობა
        //1.4. მოხდეს არსებული ჩანაწერის შენახვა

        var sortIdHelperType = typeof(SortIdHelper<>).MakeGenericType(_entityType.ClrType);
        if (Activator.CreateInstance(sortIdHelperType, _cmdRepo) is not ISortIdHelper sortHelper)
        {
            return new[] { MasterDataCrudErrors.SortIdHelperWasNotCreatedForType(_entityType.ClrType) };
        }

        _sortHelper = sortHelper;

        var queryResult = Query();
        if (queryResult.IsT1)
        {
            return queryResult.AsT1;
        }

        int sortIdMax = sortHelper.CountSortIdMax(queryResult.AsT0);

        //var newSortId = sortIdMax + itemsCount;

        var newItemWsi = (ISortedDataType)newItem;
        if (newItemWsi.SortId <= 0)
        {
            newItemWsi.SortId = sortIdMax + 1;
        }

        //2. თუ SortId > 0-ზე,
        //2.1. SortId--
        //2.2. ვიპოვოთ SortId-ის შესაბამისი ჩანაწერი არსებობს თუ არა ცხრილში. (ოღონდ ეს ჩანაწერი უნდა იყოს დასარედაქტირებელი ჩანაწერისგან განსხვავებული)
        //2.2.2. თუ არსებობს
        //2.2.2.1 ყველა ჩანაწერი, რომლი SortId >= შესანახ SortId-ს, ყველას გავუზარდოთ 1-ით
        //2.2.3 მოხდეს არსებული ჩანაწერის შენახვა არსებული SortId მნიშვნელობით
        else
        {
            int newSortId = newItemWsi.SortId - 1;
            int itemsCount = sortHelper.CountItems(queryResult.AsT0);
            int increaseWith = sortIdMax + itemsCount;
            newItemWsi.SortId += increaseWith;
            await sortHelper.IncreaseSortIds(queryResult.AsT0, newSortId, increaseWith + 2, newItemWsi.Id,
                cancellationToken);
        }

        return await Update(id, newItem, cancellationToken);

        //3. დავადგინოთ არის თუ არა ისეთი ჩანაწერები, რომლებიც იწვევს SortId-ის ჩავარდნას და გამოვასწოროთ ჩავარდნები.
        //3.1 უნდა ჩავტვირთოთ იდენტიფიკატორები, SortId-ები, RowId-ები დალაგებული SortId-ებით
        //3.2. ისეთი ჩანაწერებისათვის რომლებისთვისაც SortId != RowId, გავაახლოთ SortId, RowId-ის მნიშვნელობით.

        //sortHelper.ReSortSortIds(queryResult.AsT0);
    }

    protected override async ValueTask<Option<Err[]>> AfterUpdateData(CancellationToken cancellationToken = default)
    {
        if (_sortHelper is null)
        {
            return new[] { MasterDataCrudErrors.SortIdHelperWasNotCreatedForType(_entityType.ClrType) };
        }

        var queryResult = Query();
        if (queryResult.IsT1)
        {
            return queryResult.AsT1;
        }

        await _sortHelper.ReSortSortIds(queryResult.AsT0, cancellationToken);
        return null;
    }

    private async Task<Option<Err[]>> Update(int id, IDataType newItem, CancellationToken cancellationToken = default)
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //
        //if (idt is null)
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

    protected override async Task<Option<Err[]>> DeleteData(int id, CancellationToken cancellationToken = default)
    {
        var getOneRecordResult = await GetOneRecord(id, cancellationToken);

        if (getOneRecordResult.IsT1)
        {
            return getOneRecordResult.AsT1;
        }

        _cmdRepo.Delete(getOneRecordResult.AsT0);

        var isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsT1)
        {
            return isGridWithSortIdResult.AsT1;
        }

        bool isGridWithSortId = isGridWithSortIdResult.AsT0;

        if (!isGridWithSortId)
        {
            return null;
        }

        //3. დავადგინოთ არის თუ არა ისეთი ჩანაწერები, რომლებიც იწვევს SortId-ის ჩავარდნას და გამოვასწოროთ ჩავარდნები.
        //3.1 უნდა ჩავტვირთოთ იდენტიფიკატორები, SortId-ები, RowId-ები დალაგებული SortId-ებით
        //3.2. ისეთი ჩანაწერებისათვის რომლებისთვისაც SortId != RowId, გავაახლოთ SortId, RowId-ის მნიშვნელობით.

        var sortIdHelperType = typeof(SortIdHelper<>).MakeGenericType(_entityType.ClrType);
        if (Activator.CreateInstance(sortIdHelperType, _cmdRepo) is not ISortIdHelper sortHelper)
        {
            return new[] { MasterDataCrudErrors.SortIdHelperWasNotCreatedForType(_entityType.ClrType) };
        }

        var queryResult = Query();
        if (queryResult.IsT1)
        {
            return queryResult.AsT1;
        }

        await sortHelper.ReSortSortIds(queryResult.AsT0, cancellationToken);

        return null;
    }

    private async Task<Option<Err[]>> Validate(IDataType newItem, CancellationToken cancellationToken = default)
    {
        //var dt = _context.DataTypes.SingleOrDefault(s => s.DtTable == tableName);
        var gridModel = await GetDataTypeGridRulesByTableName(cancellationToken);

        if (gridModel is null)
        {
            return new[] { MasterDataApiErrors.MasterDataInvalidValidationRules(_tableName) };
        }

        List<Err> errors = [];
        var props = newItem.GetType().GetProperties();

        foreach (var cell in gridModel.Cells)
        {
            var prop = props.SingleOrDefault(w => w.Name == cell.FieldName);
            if (prop is null)
            {
                errors.Add(MasterDataApiErrors.MasterDataFieldNotFound(_tableName, cell.FieldName));
                continue;
            }

            var mes = cell.Validate(prop.GetValue(newItem));
            if (mes.Count > 0)
            {
                errors.AddRange(mes);
            }
        }

        return errors.Count == 0 ? null : errors.ToArray();
    }
}
