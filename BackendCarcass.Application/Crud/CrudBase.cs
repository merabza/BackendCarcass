using System;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Crud.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Crud;

public abstract class CrudBase
{
    private readonly IDatabaseAbstraction _databaseAbstraction;
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once BothContextCallDeclaration.Global
    protected CrudBase(ILogger logger, IUnitOfWork unitOfWork, IDatabaseAbstraction databaseAbstraction)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _databaseAbstraction = databaseAbstraction;
    }

    //ასეთი მიდგომა სწორია და არ უნდა შეიცვალოს, რადგან ახალი ჩანაწერის შექმნისას იდენტიფიკატორი მანამ არის 0, სანამ არ მოხდება ბაზაში შენახვა.
    //შესაბამისად შექმნის პროცედურას შეუძლია დააბრუნოს შესანახ ობიექტზე რეფერენსი და როცა უკვე შენახვა მოხდება, ამ რეფერენსიდან შესაძლებელი იქნება იდენტიფიკატორის ამოღება
    protected virtual int JustCreatedId => 0;

    // ReSharper disable once BothContextCallDeclaration.Global
    public Task<Result<ICrudData>> GetOne(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return GetOneData(id, cancellationToken);
        }
        catch (Exception e)
        {
            const string methodName = nameof(GetOne);
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "Error occurred executing {MethodName}.", methodName);
            }

            throw;
        }
    }

    public async Task<Result<ICrudData>> Create(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default)
    {
        const string methodName = nameof(Create);
        try
        {
            // ReSharper disable once using
            await using IDbContextTransaction transaction =
                await _databaseAbstraction.BeginTransactionAsync(cancellationToken);
            try
            {
                Result result = await CreateData(crudDataForCreate, cancellationToken);
                if (result.IsFailure)
                {
                    return Result.Failure<ICrudData>(result.Error);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return await GetOne(JustCreatedId, cancellationToken);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                if (e.InnerException is not null)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError(e.InnerException, "Error occurred executing {MethodName}.", methodName);
                    }

                    if (e.InnerException.Message.StartsWith("Cannot insert duplicate key row in object",
                            StringComparison.Ordinal))
                    {
                        return Result.Failure<ICrudData>(SystemToolsErrors.SuchARecordAlreadyExists);
                    }
                }

                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(e, "Error occurred executing {MethodName}.", methodName);
                }

                return Result.Failure<ICrudData>(SystemToolsErrors.UnexpectedApiException(e));
            }
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "Error occurred executing {MethodName}.", methodName);
            }

            return Result.Failure<ICrudData>(SystemToolsErrors.UnexpectedApiException(e));
        }
    }

    public async Task<Result> Update(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ReSharper disable once using
            await using IDbContextTransaction transaction =
                await _databaseAbstraction.BeginTransactionAsync(cancellationToken);
            try
            {
                Result updateDataResult = await UpdateData(id, crudDataNewVersion, cancellationToken);
                if (updateDataResult.IsFailure)
                {
                    return updateDataResult;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                Result afterUpdateDataResult = await AfterUpdateData(cancellationToken);
                if (afterUpdateDataResult.IsFailure)
                {
                    return afterUpdateDataResult;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure(SystemToolsErrors.UnexpectedApiException(e));
            }
        }
        catch (Exception e)
        {
            return Result.Failure(SystemToolsErrors.UnexpectedApiException(e));
        }
    }

    public async Task<Result> Delete(int id, CancellationToken cancellationToken = default)
    {
        const string methodName = nameof(Delete);
        try
        {
            // ReSharper disable once using
            await using IDbContextTransaction transaction =
                await _databaseAbstraction.BeginTransactionAsync(cancellationToken);
            try
            {
                Result result = await DeleteData(id, cancellationToken);
                if (result.IsFailure)
                {
                    return result;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                if (e.InnerException is not null)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError(e.InnerException, "Error occurred executing {MethodName}.", methodName);
                    }

                    if (e.InnerException.Message.StartsWith(
                            "The DELETE statement conflicted with the REFERENCE constraint", StringComparison.Ordinal))
                    {
                        return Result.Failure(SystemToolsErrors.TheEntryHasBeenUsedAndCannotBeDeleted);
                    }
                }

                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(e, "Error occurred executing {MethodName}.", methodName);
                }

                return Result.Failure(SystemToolsErrors.UnexpectedApiException(e));
            }
        }
        catch (Exception e)
        {
            return Result.Failure(SystemToolsErrors.UnexpectedApiException(e));
        }
    }

    protected abstract Task<Result<ICrudData>> GetOneData(int id, CancellationToken cancellationToken = default);

    protected abstract ValueTask<Result> CreateData(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default);

    protected abstract ValueTask<Result> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default);

    protected virtual ValueTask<Result> AfterUpdateData(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(Result.Success());
    }

    protected abstract Task<Result> DeleteData(int id, CancellationToken cancellationToken = default);

    public abstract ValueTask<Result<TableRowsData>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken = default);
}
