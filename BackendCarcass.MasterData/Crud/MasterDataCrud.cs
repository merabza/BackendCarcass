using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.LibCrud.Models;
using BackendCarcass.MasterData.CellModels;
using BackendCarcass.MasterData.Models;
using BackendCarcass.MasterData.SortIdStuff;
using BackendCarcassDomain.Entities;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Crud;

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
        ICarcassMasterDataRepository cmdRepo, IUnitOfWork unitOfWork, IDatabaseAbstraction databaseAbstraction) : base(
        logger, unitOfWork, databaseAbstraction)
    {
        _tableName = tableName;
        _entityType = entityType;
        _cmdRepo = cmdRepo;
    }

    protected override int JustCreatedId => _justCreated?.Id ?? 0;

    public async ValueTask<Result<IEnumerable<IDataType>>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        Result<IQueryable<IDataType>> queryResult = Query();
        if (queryResult.IsFailure)
        {
            return Result.Failure<IEnumerable<IDataType>>(queryResult.Error);
        }

        Result<bool> isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsFailure)
        {
            return Result.Failure<IEnumerable<IDataType>>(isGridWithSortIdResult.Error);
        }

        bool isGridWithSortId = isGridWithSortIdResult.Value;

        IQueryable<IDataType> query = queryResult.Value;

        if (!isGridWithSortId)
        {
            return await query.ToListAsync(cancellationToken);
        }

        MethodInfo? method = typeof(MasterDataCrud).GetMethod(nameof(OrderBySortId), 1, [typeof(object)]);
        MethodInfo? generic = method?.MakeGenericMethod(_entityType.ClrType);
        if (generic is null)
        {
            return Result.Failure<IEnumerable<IDataType>>(
                MasterDataCrudErrors.GenericMethodWasNotCreated(nameof(OrderBySortId)).ToError());
        }

        object? queryRunResult = generic.Invoke(this, [query]);
        if (queryRunResult is null)
        {
            return Result.Failure<IEnumerable<IDataType>>(
                MasterDataCrudErrors.MethodResultIsNull(nameof(OrderBySortId)).ToError());
        }

        return (List<IDataType>)queryRunResult;
    }

    public static Result<MasterDataCrud> Create(string tableName, ILogger logger,
        ICarcassMasterDataRepository cmdRepo, IUnitOfWork unitOfWork, IDatabaseAbstraction databaseAbstraction)
    {
        IEntityType? entityType = cmdRepo.GetEntityTypeByTableName(tableName);
        if (entityType is null)
        {
            //ვერ ვიპოვეთ შესაბამისი ცხრილი
            return Result.Failure<MasterDataCrud>(MasterDataApiErrors.TableNotFound(tableName).ToError());
        }

        return new MasterDataCrud(tableName, entityType, logger, cmdRepo, unitOfWork, databaseAbstraction);
    }

    private async Task<Result<bool>> IsGridWithSortId(CancellationToken cancellationToken = default)
    {
        GridModel? gridModel = await GetDataTypeGridRulesByTableName(cancellationToken);
        if (gridModel is null)
        {
            return Result.Failure<bool>(MasterDataCrudErrors.GridModelIsNull(_tableName).ToError());
        }

        IntegerCell? sortIdCell = null;
        foreach (Cell cell in gridModel.Cells.Where(x => x.TypeName == "Integer"))
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
        return
        [
            .. tQuery.OrderBy(x => x.SortId).Select(delegate(T s)
            {
                //s.SortId++;
                return (IDataType)s;
            })
        ];
    }

    public override async ValueTask<Result<TableRowsData>> GetTableRowsData(
        FilterSortRequest filterSortRequest, CancellationToken cancellationToken = default)
    {
        Result<object> queryResult = QueryObject();
        if (queryResult.IsFailure)
        {
            return Result.Failure<TableRowsData>(queryResult.Error);
        }

        object query = queryResult.Value;

        MethodInfo? method = typeof(MasterDataCrud).GetMethod(nameof(UseCustomSortFilterPagination), 1,
            [typeof(object), typeof(FilterSortRequest), typeof(CancellationToken)]);
        //var method = typeof(MasterDataCrud).GetMethod(nameof(UseUseCustomSortFilterPagination));
        MethodInfo? generic = method?.MakeGenericMethod(_entityType.ClrType);
        if (generic is null)
        {
            return Result.Failure<TableRowsData>(
                MasterDataCrudErrors.GenericMethodWasNotCreated(nameof(UseCustomSortFilterPagination)).ToError());
        }

        // ReSharper disable once using
        using var result = (Task<TableRowsData>?)generic.Invoke(this, [query, filterSortRequest, cancellationToken]);
        if (result is null)
        {
            return Result.Failure<TableRowsData>(
                MasterDataCrudErrors.MethodResultTaskIsNull(nameof(UseCustomSortFilterPagination)).ToError());
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
            GridModel? gridModel = await GetDataTypeGridRulesByTableName(cancellationToken);
            //DtNameFieldName
            if (gridModel is not null)
            {
                foreach (SortField sortField in filterSortRequest.SortByFields)
                {
                    Cell? cell = gridModel.Cells.SingleOrDefault(x => x.FieldName == sortField.FieldName);

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

        (int realOffset, int count, List<dynamic> rows) = await tQuery.UseCustomSortFilterPagination(filterSortRequest,
            s => s.EditFields(), cancellationToken);
        return new TableRowsData(count, realOffset, rows);
    }

    protected override async Task<Result<ICrudData>> GetOneData(int id,
        CancellationToken cancellationToken = default)
    {
        Result<IDataType> getOneRecordResult = await GetOneRecord(id, cancellationToken);
        if (getOneRecordResult.IsFailure)
        {
            return Result.Failure<ICrudData>(getOneRecordResult.Error);
        }

        Result<bool> isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsFailure)
        {
            return Result.Failure<ICrudData>(isGridWithSortIdResult.Error);
        }

        bool isGridWithSortId = isGridWithSortIdResult.Value;

        if (!isGridWithSortId)
        {
            return new MasterDataCrudLoadedData(getOneRecordResult.Value.EditFields());
        }

        var sortedData = (ISortedDataType)getOneRecordResult.Value;
        sortedData.SortId++;
        return new MasterDataCrudLoadedData(sortedData.EditFields());
    }

    private async Task<Result<IDataType>> GetOneRecord(int id, CancellationToken cancellationToken = default)
    {
        Result<IQueryable<IDataType>> entResult = Query();
        if (entResult.IsFailure)
        {
            return Result.Failure<IDataType>(entResult.Error);
        }

        IQueryable<IDataType> res = entResult.Value;

        Result<string> keyResult = GetSingleKeyPropertyName();
        if (keyResult.IsFailure)
        {
            return Result.Failure<IDataType>(keyResult.Error);
        }

        string keyPropertyName = keyResult.Value;

        ParameterExpression parameter = Expression.Parameter(_entityType.ClrType, keyPropertyName);
        ConstantExpression constant = Expression.Constant(id);
        BinaryExpression equal = Expression.Equal(parameter, constant);
        Expression<Func<IDataType, bool>> lambda = Expression.Lambda<Func<IDataType, bool>>(equal, parameter);
        IDataType? idt = await res.Where(lambda).SingleOrDefaultAsync(cancellationToken);

        if (idt is not null)
        {
            return Result.Success(idt);
        }

        return Result.Failure<IDataType>(MasterDataApiErrors.EntryNotFound().ToError());
    }

    private Result<string> GetSingleKeyPropertyName()
    {
        IKey? singleKey = _entityType.GetKeys().SingleOrDefault();
        if (singleKey is null)
        {
            //ვერ ვიპოვეთ ერთადერთი გასაღები
            return Result.Failure<string>(MasterDataApiErrors.TableHaveNotSingleKey(_tableName).ToError());
        }

        if (singleKey.Properties.Count != 1)
        {
            //ვერ ვიპოვეთ ერთადერთი გასაღები
            return Result.Failure<string>(MasterDataApiErrors.TableSingleKeyMustHaveOneProperty(_tableName).ToError());
        }

        return singleKey.Properties[0].Name;
    }

    private Result<object> QueryObject()
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //

        //return _cmdRepo.LoadByTableName(_tableName);

        MethodInfo? setMethod = _cmdRepo.SetMethodInfo();
        if (setMethod is null)
        {
            //ცხრილს არ აქვს მეთოდი Set
            return Result.Failure<object>(MasterDataApiErrors.SetMethodNotFoundForTable(_tableName).ToError());
        }

        object? result = _cmdRepo.RunGenericMethodForLoadAllRecords(setMethod, _entityType);
        return result is null
            //ცხრილის Set მეთოდი აბრუნებს null-ს
            ? Result.Failure<object>(MasterDataApiErrors.SetMethodReturnsNullForTable(_tableName).ToError())
            : Result.Success(result);
    }

    private Result<IQueryable<IDataType>> Query()
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //

        //return _cmdRepo.LoadByTableName(_tableName);

        MethodInfo? setMethod = _cmdRepo.SetMethodInfo();
        if (setMethod is null)
        {
            //ცხრილს არ აქვს მეთოდი Set
            return Result.Failure<IQueryable<IDataType>>(
                MasterDataApiErrors.SetMethodNotFoundForTable(_tableName).ToError());
        }

        object? result = _cmdRepo.RunGenericMethodForLoadAllRecords(setMethod, _entityType);
        return result is null
            //ცხრილის Set მეთოდი აბრუნებს null-ს
            ? Result.Failure<IQueryable<IDataType>>(
                MasterDataApiErrors.SetMethodReturnsNullForTable(_tableName).ToError())
            : Result.Success((IQueryable<IDataType>)result);
    }

    protected override async ValueTask<Option<ErrorOmd[]>> CreateData(ICrudData crudDataForCreate,
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

        Option<ErrorOmd[]> validateResult = await Validate(newItem, cancellationToken);
        if (validateResult.IsSome)
        {
            return (ErrorOmd[])validateResult;
        }

        Result<bool> isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsFailure)
        {
            return isGridWithSortIdResult.Error.ToErrorOmdArray();
        }

        bool isGridWithSortId = isGridWithSortIdResult.Value;

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

        Type sortIdHelperType = typeof(SortIdHelper<>).MakeGenericType(_entityType.ClrType);
        if (Activator.CreateInstance(sortIdHelperType, _cmdRepo) is not ISortIdHelper sortHelper)
        {
            return new[] { MasterDataCrudErrors.SortIdHelperWasNotCreatedForType(_entityType.ClrType) };
        }

        Result<IQueryable<IDataType>> queryResult = Query();
        if (queryResult.IsFailure)
        {
            return queryResult.Error.ToErrorOmdArray();
        }

        //მაქსიმუმის დათვლა სხვადასხვა მიზეზებით გვჭირდება, ამიტომ ვითვლით აქ.
        int sortIdMax = sortHelper.CountSortIdMax(queryResult.Value);

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
            if (await sortHelper.IsSortIdExists(queryResult.Value, newItemWsi.SortId, 0))
            {
                await sortHelper.IncreaseSortIds(queryResult.Value, newItemWsi.SortId, sortIdMax, 0,
                    cancellationToken);
            }
        }
        //3. დავადგინოთ არის თუ არა ისეთი ჩანაწერები, რომლებიც იწვევს SortId-ის ჩავარდნას და გამოვასწოროთ ჩავარდნები.
        //3.1 უნდა ჩავტვირთოთ იდენტიფიკატორები, SortId-ები, RowId-ები დალაგებული SortId-ებით
        //3.2. ისეთი ჩანაწერებისათვის რომლებისთვისაც SortId != RowId, გავაახლოთ SortId, RowId-ის მნიშვნელობით.

        //await sortHelper.ReSortSortIds(queryResult.AsT0, cancellationToken);

        await _cmdRepo.Create(newItem, cancellationToken);
        _justCreated = newItem;
        return null;

        //return createResult.Match(x => x, () => OneOf<IDataType, ErrorOmd[]>.FromT0(newItem));
    }

    protected override async ValueTask<Option<ErrorOmd[]>> UpdateData(int id, ICrudData crudDataNewVersion,
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
            return new[]
            {
                MasterDataApiErrors.WrongId(_tableName)
            }; //მოწოდებული ინფორმაცია არასწორია, რადგან იდენტიფიკატორი არ ემთხვევა მოწოდებული ობიექტის იდენტიფიკატორს
        }

        Option<ErrorOmd[]> validateResult = await Validate(newItem, cancellationToken);
        if (validateResult.IsSome)
        {
            return validateResult;
        }

        Result<bool> isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsFailure)
        {
            return isGridWithSortIdResult.Error.ToErrorOmdArray();
        }

        bool isGridWithSortId = isGridWithSortIdResult.Value;

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

        Type sortIdHelperType = typeof(SortIdHelper<>).MakeGenericType(_entityType.ClrType);
        if (Activator.CreateInstance(sortIdHelperType, _cmdRepo) is not ISortIdHelper sortHelper)
        {
            return new[] { MasterDataCrudErrors.SortIdHelperWasNotCreatedForType(_entityType.ClrType) };
        }

        _sortHelper = sortHelper;

        Result<IQueryable<IDataType>> queryResult = Query();
        if (queryResult.IsFailure)
        {
            return queryResult.Error.ToErrorOmdArray();
        }

        int sortIdMax = sortHelper.CountSortIdMax(queryResult.Value);

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
            int itemsCount = sortHelper.CountItems(queryResult.Value);
            int increaseWith = sortIdMax + itemsCount;
            newItemWsi.SortId += increaseWith;
            await sortHelper.IncreaseSortIds(queryResult.Value, newSortId, increaseWith + 2, newItemWsi.Id,
                cancellationToken);
        }

        return await Update(id, newItem, cancellationToken);

        //3. დავადგინოთ არის თუ არა ისეთი ჩანაწერები, რომლებიც იწვევს SortId-ის ჩავარდნას და გამოვასწოროთ ჩავარდნები.
        //3.1 უნდა ჩავტვირთოთ იდენტიფიკატორები, SortId-ები, RowId-ები დალაგებული SortId-ებით
        //3.2. ისეთი ჩანაწერებისათვის რომლებისთვისაც SortId != RowId, გავაახლოთ SortId, RowId-ის მნიშვნელობით.

        //sortHelper.ReSortSortIds(queryResult.AsT0);
    }

    protected override async ValueTask<Option<ErrorOmd[]>> AfterUpdateData(
        CancellationToken cancellationToken = default)
    {
        if (_sortHelper is null)
        {
            return new[] { MasterDataCrudErrors.SortIdHelperWasNotCreatedForType(_entityType.ClrType) };
        }

        Result<IQueryable<IDataType>> queryResult = Query();
        if (queryResult.IsFailure)
        {
            return queryResult.Error.ToErrorOmdArray();
        }

        await _sortHelper.ReSortSortIds(queryResult.Value, cancellationToken);
        return null;
    }

    private async Task<Option<ErrorOmd[]>> Update(int id, IDataType newItem,
        CancellationToken cancellationToken = default)
    {
        //var q = _cmdRepo.RunGenericMethodForQueryRecords(entityType);
        //var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //
        //if (idt is null)
        //    return new[]
        //    {
        //        MasterDataApiErrors.RecordNotFound(_tableName, id)
        //    }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound

        Result<IDataType> result = await GetOneRecord(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToErrorOmdArray();
        }

        IDataType record = result.Value;
        record.UpdateTo(newItem);
        _cmdRepo.Update(record);
        return null;
    }

    protected override async Task<Option<ErrorOmd[]>> DeleteData(int id, CancellationToken cancellationToken = default)
    {
        Result<IDataType> getOneRecordResult = await GetOneRecord(id, cancellationToken);

        if (getOneRecordResult.IsFailure)
        {
            return getOneRecordResult.Error.ToErrorOmdArray();
        }

        _cmdRepo.Delete(getOneRecordResult.Value);

        Result<bool> isGridWithSortIdResult = await IsGridWithSortId(cancellationToken);
        if (isGridWithSortIdResult.IsFailure)
        {
            return isGridWithSortIdResult.Error.ToErrorOmdArray();
        }

        bool isGridWithSortId = isGridWithSortIdResult.Value;

        if (!isGridWithSortId)
        {
            return null;
        }

        //3. დავადგინოთ არის თუ არა ისეთი ჩანაწერები, რომლებიც იწვევს SortId-ის ჩავარდნას და გამოვასწოროთ ჩავარდნები.
        //3.1 უნდა ჩავტვირთოთ იდენტიფიკატორები, SortId-ები, RowId-ები დალაგებული SortId-ებით
        //3.2. ისეთი ჩანაწერებისათვის რომლებისთვისაც SortId != RowId, გავაახლოთ SortId, RowId-ის მნიშვნელობით.

        Type sortIdHelperType = typeof(SortIdHelper<>).MakeGenericType(_entityType.ClrType);
        if (Activator.CreateInstance(sortIdHelperType, _cmdRepo) is not ISortIdHelper sortHelper)
        {
            return new[] { MasterDataCrudErrors.SortIdHelperWasNotCreatedForType(_entityType.ClrType) };
        }

        Result<IQueryable<IDataType>> queryResult = Query();
        if (queryResult.IsFailure)
        {
            return queryResult.Error.ToErrorOmdArray();
        }

        await sortHelper.ReSortSortIds(queryResult.Value, cancellationToken);

        return null;
    }

    private async Task<Option<ErrorOmd[]>> Validate(IDataType newItem, CancellationToken cancellationToken = default)
    {
        //var dt = _context.DataTypes.SingleOrDefault(s => s.DtTable == tableName);
        GridModel? gridModel = await GetDataTypeGridRulesByTableName(cancellationToken);

        if (gridModel is null)
        {
            return new[] { MasterDataApiErrors.MasterDataInvalidValidationRules(_tableName) };
        }

        List<ErrorOmd> errors = [];
        PropertyInfo[] props = newItem.GetType().GetProperties();

        foreach (Cell cell in gridModel.Cells)
        {
            PropertyInfo? prop = props.SingleOrDefault(w => w.Name == cell.FieldName);
            if (prop is null)
            {
                errors.Add(MasterDataApiErrors.MasterDataFieldNotFound(_tableName, cell.FieldName));
                continue;
            }

            List<ErrorOmd> mes = cell.Validate(prop.GetValue(newItem));
            if (mes.Count > 0)
            {
                errors.AddRange(mes);
            }
        }

        return errors.Count == 0 ? null : errors.ToArray();
    }
}
