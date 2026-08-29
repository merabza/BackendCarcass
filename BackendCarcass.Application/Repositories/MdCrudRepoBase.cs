using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BackendCarcass.Domain;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Repositories;

public sealed class MdCrudRepoBase(ICarcassApplicationDbContext carcassContext, string tableName) : IMdCrudRepo
{
    public Result<IQueryable<IDataType>> Load()
    {
        IEntityType? vvv = carcassContext.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
        if (vvv == null)
        {
            //ვერ ვიპოვეთ შესაბამისი ცხრილი
            return Result.Failure<IQueryable<IDataType>>(MasterDataApiErrors.TableNotFound(tableName).ToError());
        }

        MethodInfo? setMethod = carcassContext.GetType().GetMethod("Set", []);
        if (setMethod == null)
        {
            //ცხრილს არ აქვს მეთოდი Set
            return Result.Failure<IQueryable<IDataType>>(MasterDataApiErrors.SetMethodNotFoundForTable(tableName)
                .ToError());
        }

        object? result = setMethod.MakeGenericMethod(vvv.ClrType).Invoke(carcassContext, null);
        return result == null
            //ცხრილის Set მეთოდი აბრუნებს null-ს
            ? Result.Failure<IQueryable<IDataType>>(MasterDataApiErrors.SetMethodReturnsNullForTable(tableName)
                .ToError())
            : Result.Success((IQueryable<IDataType>)result);
    }

    public async Task<Result> Create(IDataType newItem)
    {
        await carcassContext.AddAsync(newItem);
        await carcassContext.SaveChangesAsync();
        return Result.Success();
    }

    public async ValueTask<Result> Update(int id, IDataType newItem)
    {
        IEntityType? vvv = carcassContext.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
        if (vvv == null)
        {
            return Result.Failure(MasterDataApiErrors.TableNotFound(tableName)
                .ToError()); //ვერ ვიპოვეთ შესაბამისი ცხრილი
        }

        var q = (IQueryable<IDataType>?)carcassContext.GetType().GetMethod("Set")?.MakeGenericMethod(vvv.ClrType)
            .Invoke(carcassContext, null);
        IDataType? idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id);
        if (idt == null)
        {
            //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound
            return Result.Failure(MasterDataApiErrors.RecordNotFound(tableName, id).ToError());
        }

        idt.UpdateTo(newItem);

        carcassContext.Update(idt);
        await carcassContext.SaveChangesAsync();
        return Result.Success();
    }

    public async ValueTask<Result> Delete(int id)
    {
        Result<IQueryable<IDataType>> entResult = Load();
        if (entResult.IsFailure)
        {
            return Result.Failure(entResult.Error);
        }

        List<IDataType> res = await entResult.Value.ToListAsync(); // S6966: Await ToListAsync instead.
        IDataType? idt = res.SingleOrDefault(w => w.Id == id);
        if (idt == null)
        {
            //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound
            return Result.Failure(MasterDataApiErrors.RecordNotFound(tableName, id).ToError());
        }

        carcassContext.Remove(id);
        await carcassContext.SaveChangesAsync();
        return Result.Success();
    }
}
