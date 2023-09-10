using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace LibCrud;

public abstract class CrudBase
{
    private readonly IAbstractRepository _absRepo;
    protected readonly ILogger Logger;
    protected int JustCreatedId;

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
        const string methodName = nameof(GetOne);
        try
        {
            await using var transaction = await _absRepo.GetTransaction(cancellationToken);
            try
            {
                if (await CreateData(crudDataForCreate, cancellationToken))
                {
                    await _absRepo.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return await GetOne(JustCreatedId, cancellationToken);
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                Logger.LogError(e, "Error occurred executing {methodName}.", methodName);
                return new[] { Errors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error occurred executing {methodName}.", methodName);
            return new[] { Errors.UnexpectedApiException(e) };
        }

        return new[] { Errors.UnexpectedError };
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
}