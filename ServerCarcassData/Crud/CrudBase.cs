using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServerCarcassData.Crud;

public /*open*/ class CrudBase
{
    private readonly ICrudRepository _repo;
    protected readonly ILogger Logger;

    protected CrudBase(ILogger logger, ICrudRepository repo)
    {
        Logger = logger;
        _repo = repo;
    }

    public string? LastMessage { get; protected set; }

    public Task<ICrudData?> GetOne(int id)
    {
        try
        {
            return GetOneData(id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occurred executing {nameof(GetOne)}.");
            throw;
        }
    }

    private string? CheckValidate(ICrudData? crudData, int? id = null)
    {
        if (crudData == null)
            return "ატვირთული ინფორმაციის გაშიფვრა ვერ მოხერხდა.";
        if (id != null && crudData.Id != id)
            return "ატვირთული ინფორმაცია არასწორია. (არასწორი იდენტიფიკატორი.)";

        //(ჯერჯერობით ანალოგიურის შემოწმებას არ ვაკეთებ, რადგან საკმაოდ რთული მექანიძმი იქნება ასაწყობი.
        //მომავლისთვის გადავდე და არ ვიცი საერთოდ თუ გაკეთდება)
        //შემოწმდეს ხომ არ არსებობს ანალოგიური ჩანაწერი ბაზაში
        //ExistsSimilar

        //დანარჩენი შემოწმება უკვე თვითონ ობიექტის დონეზე მოხდეს
        return crudData.CheckValidateData(_repo, id);
    }

    public async Task<ICrudData?> Create(Stream bodyStream)
    {
        try
        {
            var crudDataForCreate = DeserializeCrudData(bodyStream);

            if (crudDataForCreate is null)
                return null;
            //1. შემოწმდეს მოწოდებული ინფორმაცია
            if ((LastMessage = CheckValidate(crudDataForCreate)) != null)
                return null;

            await using var transaction = _repo.Transaction();
            try
            {
                if (CreateData(crudDataForCreate))
                {
                    await _repo.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return await GetOne(JustCreatedId);
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                Logger.LogError(e, $"Error occurred executing {nameof(Create)}.");
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occurred executing {nameof(Create)}.");
            throw;
        }

        return null;
    }

    private ICrudData? DeserializeCrudData(Stream bodyStream)
    {
        using StreamReader reader = new(bodyStream);
        var body = reader.ReadToEnd();
        return DeserializeData(body);
    }

    public async Task<bool> AfterUpdate()
    {
        try
        {
            await using var transaction = _repo.Transaction();
            try
            {
                if (AfterUpdateData())
                {
                    await _repo.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occurred executing {nameof(AfterUpdate)}.");
            throw;
        }

        return false;
    }

    public async Task<bool> Update(int id, Stream bodyStream)
    {
        try
        {
            var crudDataNewVersion = DeserializeCrudData(bodyStream);
            if (crudDataNewVersion is null)
                return false;
            //1. შემოწმდეს მოწოდებული ინფორმაცია
            if ((LastMessage = CheckValidate(crudDataNewVersion, id)) != null)
                return false;

            await using var transaction = _repo.Transaction();
            try
            {
                if (UpdateData(id, crudDataNewVersion))
                {
                    await _repo.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occurred executing {nameof(Update)}.");
            throw;
        }

        return false;
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            await using var transaction = _repo.Transaction();
            try
            {
                if (DeleteData(id))
                {
                    await _repo.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error occurred executing {nameof(Delete)}.");
            throw;
        }

        return false;
    }

    #region virtuals

    protected virtual int JustCreatedId => 0;

    protected virtual async Task<ICrudData?> GetOneData(int id)
    {
        return await Task.FromResult<ICrudData?>(null);
    }

    protected virtual ICrudData? DeserializeData(string body)
    {
        return null;
    }

    protected virtual bool CreateData(ICrudData crudDataForCreate)
    {
        return false;
    }

    protected virtual bool UpdateData(int id, ICrudData crudDataNewVersion)
    {
        return false;
    }

    protected virtual bool ConfirmRejectData(int id, bool confirm, bool withAllDescendants)
    {
        return false;
    }

    protected virtual bool AfterUpdateData()
    {
        return true;
    }

    protected virtual bool DeleteData(int id)
    {
        return false;
    }

    #endregion
}