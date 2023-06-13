using System;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace LibCrud;

public abstract class CrudBase // : ICrud
{
    private readonly IAbstractRepository _absRepo;
    protected readonly ILogger Logger;
    protected int JustCreatedId;

    //public string? LastMessage { get; protected set; }

    protected CrudBase(ILogger logger, IAbstractRepository absRepo)
    {
        Logger = logger;
        _absRepo = absRepo;
    }


    public async Task<OneOf<ICrudData, Err[]>> GetOne(int id)
    {
        try
        {
            return await GetOneData(id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occurred executing {nameof(GetOne)}.");
            throw;
        }
    }

    public async Task<OneOf<ICrudData, Err[]>> Create(ICrudData crudDataForCreate)
    {
        try
        {
            await using var transaction = _absRepo.GetTransaction();
            try
            {
                if (await CreateData(crudDataForCreate))
                {
                    await _absRepo.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return await GetOne(JustCreatedId);
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                Logger.LogError(e, $"Error occurred executing {nameof(Create)}.");
                return new[] { Errors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occurred executing {nameof(Create)}.");
            return new[] { Errors.UnexpectedApiException(e) };
        }

        return new[] { Errors.UnexpectedError };
    }

    public async Task<Option<Err[]>> Update(int id, ICrudData crudDataNewVersion)
    {
        try
        {
            await using var transaction = _absRepo.GetTransaction();
            try
            {
                var result = await UpdateData(id, crudDataNewVersion);
                if (result.IsSome)
                    return result;
                await _absRepo.SaveChangesAsync();
                await transaction.CommitAsync();
                return null;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return new[] { Errors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            return new[] { Errors.UnexpectedApiException(e) };
        }
    }

    public async Task<Option<Err[]>> Delete(int id)
    {
        try
        {
            await using var transaction = _absRepo.GetTransaction();
            try
            {
                var result = await DeleteData(id);
                if (result.IsSome)
                    return result;
                await _absRepo.SaveChangesAsync();
                await transaction.CommitAsync();
                return null;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return new[] { Errors.UnexpectedApiException(e) };
            }
        }
        catch (Exception e)
        {
            return new[] { Errors.UnexpectedApiException(e) };
        }
    }


    protected abstract Task<OneOf<ICrudData, Err[]>> GetOneData(int id);
    //{
    //    return await Task.FromResult(new[] { CrudApiErrors.VirtualMethodDoesNotImplemented });
    //}

    protected abstract Task<Option<Err[]>> CreateData(ICrudData crudDataForCreate);

    protected abstract Task<Option<Err[]>> UpdateData(int id, ICrudData crudDataNewVersion);

    protected abstract Task<Option<Err[]>> DeleteData(int id);
}