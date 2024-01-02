using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LibCrud.Models;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
// ReSharper disable ConvertToPrimaryConstructor

namespace LibCrud;

public abstract class CrudBase
{
    private readonly IAbstractRepository _absRepo;
    protected readonly ILogger Logger;

    //ასეთი მიდგომა სწორია და არ უნდა შეიცვალოს, რადგან ახალი ჩანაწერის შექმნისას იდენტიფიკატორი მანამ არის 0, სანამ არ მოხდება ბაზაში შენახვა.
    //შესაბამისად შექმნის პროცედურას შეუძლია დააბრუნოს შესანახ ობიექტზე რეფერენსი და როცა უკვე შენახვა მოხდება, ამ რეფერენსიდან შესაძლებელი იქნება იდენტიფიკატორის ამოღება
    protected virtual int JustCreatedId => 0;


    protected CrudBase(ILogger logger, IAbstractRepository absRepo)
    {
        Logger = logger;
        _absRepo = absRepo;
    }

    public async Task<OneOf<ICrudData, Err[]>> GetOne(int id, CancellationToken cancellationToken)
    {
        try
        {
            return await GetOneData(id, cancellationToken);
        }
        catch (Exception e)
        {
            const string methodName = nameof(GetOne);
            Logger.LogError(e, "Error occurred executing {methodName}.", methodName);
            throw;
        }
    }

    public async Task<OneOf<ICrudData, Err[]>> Create(ICrudData crudDataForCreate, CancellationToken cancellationToken)
    {
        const string methodName = nameof(Create);
        try
        {
            await using var transaction = await _absRepo.GetTransaction(cancellationToken);
            try
            {
                var result = await CreateData(crudDataForCreate, cancellationToken);
                if (result.IsSome)
                    return (Err[])result;
                await _absRepo.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return await GetOne(JustCreatedId, cancellationToken);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                if (e.InnerException is not null)
                {
                    Logger.LogError(e.InnerException, "Error occurred executing {methodName}.", methodName);
                    if (e.InnerException.Message.StartsWith("Cannot insert duplicate key row in object"))
                        return new[] { Errors.SuchARecordAlreadyExists };
                }

                Logger.LogError(e, "Error occurred executing {methodName}.", methodName);
                return new[] { Errors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error occurred executing {methodName}.", methodName);
            return new[] { Errors.UnexpectedApiException(e) };
        }
    }

    public async Task<Option<Err[]>> Update(int id, ICrudData crudDataNewVersion, CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await _absRepo.GetTransaction(cancellationToken);
            try
            {
                var result = await UpdateData(id, crudDataNewVersion, cancellationToken);
                if (result.IsSome)
                    return result;
                await _absRepo.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return null;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new[] { Errors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            return new[] { Errors.UnexpectedApiException(e) };
        }
    }

    public async Task<Option<Err[]>> Delete(int id, CancellationToken cancellationToken)
    {
        const string methodName = nameof(Delete);
        try
        {
            await using var transaction = await _absRepo.GetTransaction(cancellationToken);
            try
            {
                var result = await DeleteData(id, cancellationToken);
                if (result.IsSome)
                    return result;
                await _absRepo.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return null;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                if (e.InnerException is not null)
                {
                    Logger.LogError(e.InnerException, "Error occurred executing {methodName}.", methodName);
                    if (e.InnerException.Message.StartsWith(
                            "The DELETE statement conflicted with the REFERENCE constraint"))
                        return new[] { Errors.TheEntryHasBeenUsedAndCannotBeDeleted };
                }

                Logger.LogError(e, "Error occurred executing {methodName}.", methodName);
                return new[] { Errors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            return new[] { Errors.UnexpectedApiException(e) };
        }
    }

    protected abstract Task<OneOf<ICrudData, Err[]>> GetOneData(int id, CancellationToken cancellationToken);

    protected abstract Task<Option<Err[]>> CreateData(ICrudData crudDataForCreate, CancellationToken cancellationToken);

    protected abstract Task<Option<Err[]>> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken);

    protected abstract Task<Option<Err[]>> DeleteData(int id, CancellationToken cancellationToken);

    public abstract Task<OneOf<TableRowsData, Err[]>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken);
}