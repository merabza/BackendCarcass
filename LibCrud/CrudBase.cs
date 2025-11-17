using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LibCrud.Models;
using Microsoft.Extensions.Logging;
using OneOf;
using RepositoriesDom;
using SystemToolsShared.Errors;

namespace LibCrud;

public abstract class CrudBase
{
    private readonly IAbstractRepository _absRepo;
    private readonly ILogger _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected CrudBase(ILogger logger, IAbstractRepository absRepo)
    {
        _logger = logger;
        _absRepo = absRepo;
    }

    //ასეთი მიდგომა სწორია და არ უნდა შეიცვალოს, რადგან ახალი ჩანაწერის შექმნისას იდენტიფიკატორი მანამ არის 0, სანამ არ მოხდება ბაზაში შენახვა.
    //შესაბამისად შექმნის პროცედურას შეუძლია დააბრუნოს შესანახ ობიექტზე რეფერენსი და როცა უკვე შენახვა მოხდება, ამ რეფერენსიდან შესაძლებელი იქნება იდენტიფიკატორის ამოღება
    protected virtual int JustCreatedId => 0;

    public Task<OneOf<ICrudData, Err[]>> GetOne(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return GetOneData(id, cancellationToken);
        }
        catch (Exception e)
        {
            const string methodName = nameof(GetOne);
            _logger.LogError(e, "Error occurred executing {methodName}.", methodName);
            throw;
        }
    }

    public async Task<OneOf<ICrudData, Err[]>> Create(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default)
    {
        const string methodName = nameof(Create);
        try
        {
            // ReSharper disable once using
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
                    _logger.LogError(e.InnerException, "Error occurred executing {methodName}.", methodName);
                    if (e.InnerException.Message.StartsWith("Cannot insert duplicate key row in object"))
                        return new[] { SystemToolsErrors.SuchARecordAlreadyExists };
                }

                _logger.LogError(e, "Error occurred executing {methodName}.", methodName);
                return new[] { SystemToolsErrors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred executing {methodName}.", methodName);
            return new[] { SystemToolsErrors.UnexpectedApiException(e) };
        }
    }

    public async Task<Option<Err[]>> Update(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ReSharper disable once using
            await using var transaction = await _absRepo.GetTransaction(cancellationToken);
            try
            {
                var updateDataResult = await UpdateData(id, crudDataNewVersion, cancellationToken);
                if (updateDataResult.IsSome)
                    return updateDataResult;
                await _absRepo.SaveChangesAsync(cancellationToken);
                var afterUpdateDataResult = await AfterUpdateData(cancellationToken);
                if (afterUpdateDataResult.IsSome)
                    return afterUpdateDataResult;
                await _absRepo.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return null;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new[] { SystemToolsErrors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            return new[] { SystemToolsErrors.UnexpectedApiException(e) };
        }
    }

    public async Task<Option<Err[]>> Delete(int id, CancellationToken cancellationToken = default)
    {
        const string methodName = nameof(Delete);
        try
        {
            // ReSharper disable once using
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
                    _logger.LogError(e.InnerException, "Error occurred executing {methodName}.", methodName);
                    if (e.InnerException.Message.StartsWith(
                            "The DELETE statement conflicted with the REFERENCE constraint"))
                        return new[] { SystemToolsErrors.TheEntryHasBeenUsedAndCannotBeDeleted };
                }

                _logger.LogError(e, "Error occurred executing {methodName}.", methodName);
                return new[] { SystemToolsErrors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            return new[] { SystemToolsErrors.UnexpectedApiException(e) };
        }
    }

    protected abstract Task<OneOf<ICrudData, Err[]>> GetOneData(int id, CancellationToken cancellationToken = default);

    protected abstract ValueTask<Option<Err[]>> CreateData(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default);

    protected abstract ValueTask<Option<Err[]>> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default);

    protected virtual ValueTask<Option<Err[]>> AfterUpdateData(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<Option<Err[]>>(null);
    }

    protected abstract Task<Option<Err[]>> DeleteData(int id, CancellationToken cancellationToken = default);

    public abstract ValueTask<OneOf<TableRowsData, Err[]>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken = default);
}