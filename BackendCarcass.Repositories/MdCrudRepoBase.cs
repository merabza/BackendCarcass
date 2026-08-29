using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BackendCarcass.Database;
using BackendCarcassDomain.Entities;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public sealed class MdCrudRepoBase(CarcassDbContext carcassContext, string tableName) : IMdCrudRepo
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

    public async Task<Option<ErrorOmd[]>> Create(IDataType newItem)
    {
        await carcassContext.AddAsync(newItem);
        await carcassContext.SaveChangesAsync();
        return null;
    }

    public async ValueTask<Option<ErrorOmd[]>> Update(int id, IDataType newItem)
    {
        IEntityType? vvv = carcassContext.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
        if (vvv == null)
        {
            return new[] { MasterDataApiErrors.TableNotFound(tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი
        }

        var q = (IQueryable<IDataType>?)carcassContext.GetType().GetMethod("Set")?.MakeGenericMethod(vvv.ClrType)
            .Invoke(carcassContext, null);
        IDataType? idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id);
        if (idt == null)
        {
            return new[]
            {
                MasterDataApiErrors.RecordNotFound(tableName, id)
            }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound
        }

        idt.UpdateTo(newItem);

        carcassContext.Update(idt);
        await carcassContext.SaveChangesAsync();
        return null;
    }

    public async ValueTask<Option<ErrorOmd[]>> Delete(int id)
    {
        Result<IQueryable<IDataType>> entResult = Load();
        if (entResult.IsFailure)
        {
            return entResult.Error.ToErrorOmdArray();
        }

        List<IDataType> res = await entResult.Value.ToListAsync(); // S6966: Await ToListAsync instead.
        IDataType? idt = res.SingleOrDefault(w => w.Id == id);
        if (idt == null)
        {
            return new[]
            {
                MasterDataApiErrors.RecordNotFound(tableName, id)
            }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound
        }

        carcassContext.Remove(id);
        await carcassContext.SaveChangesAsync();
        return null;
    }
}
