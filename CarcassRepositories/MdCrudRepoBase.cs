using System;
using System.Linq;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassDb;
using CarcassMasterDataDom;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SystemToolsShared;

namespace CarcassRepositories;

public sealed class MdCrudRepoBase : IMdCrudRepo
{
    private readonly CarcassDbContext _carcassContext;
    private readonly string _tableName;

    public MdCrudRepoBase(CarcassDbContext carcassContext, string tableName)
    {
        _carcassContext = carcassContext;
        _tableName = tableName;
    }

    public OneOf<IQueryable<IDataType>, Err[]> Load()
    {
        var vvv = _carcassContext.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == _tableName);
        if (vvv == null)
            return new[] { MasterDataApiErrors.TableNotFound(_tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

        var setMethod = _carcassContext.GetType().GetMethod("Set", Array.Empty<Type>());
        if (setMethod == null)
            return new[] { MasterDataApiErrors.SetMethodNotFoundForTable(_tableName) }; //ცხრილს არ აქვს მეთოდი Set

        var result = setMethod.MakeGenericMethod(vvv.ClrType).Invoke(_carcassContext, null);
        return result == null
            ? new[]
            {
                MasterDataApiErrors.SetMethodReturnsNullForTable(_tableName)
            } //ცხრილის Set მეთოდი აბრუნებს null-ს
            : OneOf<IQueryable<IDataType>, Err[]>.FromT0((IQueryable<IDataType>)result);
    }

    public async Task<Option<Err[]>> Create(IDataType newItem)
    {
        await _carcassContext.AddAsync(newItem);
        await _carcassContext.SaveChangesAsync();
        return null;
    }

    public async Task<Option<Err[]>> Update(int id, IDataType newItem)
    {
        var vvv = _carcassContext.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == _tableName);
        if (vvv == null)
            return new[] { MasterDataApiErrors.TableNotFound(_tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

        var q = (IQueryable<IDataType>?)_carcassContext.GetType().GetMethod("Set")
            ?.MakeGenericMethod(vvv.ClrType)
            .Invoke(_carcassContext, null);
        var idt = q?.AsEnumerable().SingleOrDefault(w => w.Id == id); //
        if (idt == null)
            return new[]
            {
                MasterDataApiErrors.RecordNotFound(_tableName, id)
            }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound
        idt.UpdateTo(newItem);

        _carcassContext.Update(idt);
        await _carcassContext.SaveChangesAsync();
        return null;
    }

    public async Task<Option<Err[]>> Delete(int id)
    {
        var entResult = Load();
        if (entResult.IsT1)
            return entResult.AsT1;

        var res = entResult.AsT0.ToList();
        var idt = res.SingleOrDefault(w => w.Id == id);
        if (idt == null)
            return new[]
            {
                MasterDataApiErrors.RecordNotFound(_tableName, id)
            }; //ბაზაში ვერ ვიპოვეთ მოწოდებული იდენტიფიკატორის შესაბამისი ჩანაწერი. RecordNotFound

        _carcassContext.Remove(id);
        await _carcassContext.SaveChangesAsync();
        return null;
    }
}