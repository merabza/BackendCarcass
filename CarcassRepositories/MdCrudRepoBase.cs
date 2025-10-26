using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassDb;
using CarcassMasterDataDom;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassRepositories;

public sealed class MdCrudRepoBase(CarcassDbContext carcassContext, string tableName) : IMdCrudRepo
{
    public OneOf<IQueryable<IDataType>, Err[]> Load()
    {
        var vvv = carcassContext.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
        if (vvv == null)
            return new[] { MasterDataApiErrors.TableNotFound(tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

        var setMethod = carcassContext.GetType().GetMethod("Set", []);
        if (setMethod == null)
            return new[] { MasterDataApiErrors.SetMethodNotFoundForTable(tableName) }; //ცხრილს არ აქვს მეთოდი Set

        var result = setMethod.MakeGenericMethod(vvv.ClrType).Invoke(carcassContext, null);
        return result == null
            ? new[] { MasterDataApiErrors.SetMethodReturnsNullForTable(tableName) } //ცხრილის Set მეთოდი აბრუნებს null-ს
            : OneOf<IQueryable<IDataType>, Err[]>.FromT0((IQueryable<IDataType>)result);
    }

    public async Task<Option<Err[]>> Create(IDataType newItem)
    {
        await carcassContext.AddAsync(newItem);
        await carcassContext.SaveChangesAsync();
        return null;
    }

    public async ValueTask<Option<Err[]>> Update(int id, IDataType newItem)
    {
        var vvv = carcassContext.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
        if (vvv == null)
            return new[] { MasterDataApiErrors.TableNotFound(tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

        var q = (IQueryable<IDataType>?)carcassContext.GetType().GetMethod("Set")?.MakeGenericMethod(vvv.ClrType)
            .Invoke(carcassContext, null);
        var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //
        if (idt == null)
            return new[]
            {
                MasterDataApiErrors.RecordNotFound(tableName, id)
            }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound
        idt.UpdateTo(newItem);

        carcassContext.Update(idt);
        await carcassContext.SaveChangesAsync();
        return null;
    }

    public async ValueTask<Option<Err[]>> Delete(int id)
    {
        var entResult = Load();
        if (entResult.IsT1)
            return (Err[])entResult.AsT1;

        var res = entResult.AsT0.ToList();
        var idt = res.SingleOrDefault(w => w.Id == id);
        if (idt == null)
            return new[]
            {
                MasterDataApiErrors.RecordNotFound(tableName, id)
            }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound

        carcassContext.Remove(id);
        await carcassContext.SaveChangesAsync();
        return null;
    }
}