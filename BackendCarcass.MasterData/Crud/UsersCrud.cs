using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.LibCrud.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcassDomain.Entities;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Crud;

public sealed class UsersCrud : CrudBase, IMasterDataLoader
{
    private readonly UserManager<AppUser> _userManager;
    private AppUser? _justCreated;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UsersCrud(ILogger logger, UserManager<AppUser> userManager, IUnitOfWork unitOfWork,
        IDatabaseAbstraction databaseAbstraction) : base(logger, unitOfWork, databaseAbstraction)
    {
        _userManager = userManager;
    }

    protected override int JustCreatedId => _justCreated?.Id ?? 0;

    public async ValueTask<Result<IEnumerable<IDataType>>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        List<AppUser> users = await _userManager.Users.ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<IDataType>>(users
            .Where(x => x.UserName is not null && x.Email is not null)
            .Select(x => new UserCrudData(x.UserName!, x.FirstName, x.LastName, x.Email!)));
    }

    public override async ValueTask<Result<TableRowsData>> GetTableRowsData(
        FilterSortRequest filterSortRequest, CancellationToken cancellationToken = default)
    {
        IQueryable<AppUser> users = _userManager.Users;
        (int realOffset, int count, List<UserCrudData> rows) = await users.UseCustomSortFilterPagination(
            filterSortRequest, x => new UserCrudData(x.UserName!, x.FirstName, x.LastName, x.Email!),
            cancellationToken);

        return new TableRowsData(count, realOffset, [.. rows.Select(s => s.EditFields())]);
    }

    protected override async Task<Result<ICrudData>> GetOneData(int id,
        CancellationToken cancellationToken = default)
    {
        AppUser? appUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (appUser?.UserName is not null && appUser.Email is not null)
        {
            return new UserCrudData(appUser.UserName, appUser.FirstName, appUser.LastName, appUser.Email);
        }

        return Result.Failure<ICrudData>(MasterDataApiErrors.CannotFindUser.ToError());
    }

    protected override async ValueTask<Option<ErrorOmd[]>> CreateData(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default)
    {
        var user = (UserCrudData)crudDataForCreate;
        var appUser = new AppUser(user.UserName, user.FirstName, user.LastName) { Email = user.Email };

        //შევქმნათ როლი
        IdentityResult createResult = await _userManager.CreateAsync(appUser);
        if (!createResult.Succeeded)
        {
            return ConvertError(createResult);
        }

        _justCreated = appUser;
        return null;
    }

    protected override async ValueTask<Option<ErrorOmd[]>> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default)
    {
        AppUser? oldUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldUser is null)
        {
            return new[] { MasterDataApiErrors.CannotFindUser };
        }

        var user = (UserCrudData)crudDataNewVersion;
        oldUser.UserName = user.UserName;
        oldUser.Email = user.Email;
        oldUser.FirstName = user.FirstName;
        oldUser.LastName = user.LastName;

        IdentityResult updateResult = await _userManager.UpdateAsync(oldUser);
        if (!updateResult.Succeeded)
        {
            return ConvertError(updateResult);
        }

        if (oldUser.UserName != user.UserName)
        {
            IdentityResult setUserNameResult = await _userManager.SetUserNameAsync(oldUser, user.UserName);
            if (!setUserNameResult.Succeeded)
            {
                return ConvertError(setUserNameResult);
            }
        }

        if (oldUser.Email == user.Email)
        {
            return null;
        }

        IdentityResult setEmailResult = await _userManager.SetEmailAsync(oldUser, user.Email);
        return ConvertError(setEmailResult);
    }

    protected override async Task<Option<ErrorOmd[]>> DeleteData(int id, CancellationToken cancellationToken = default)
    {
        AppUser? oldUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldUser is null)
        {
            return new[] { MasterDataApiErrors.CannotFindUser };
        }

        IdentityResult deleteResult = await _userManager.DeleteAsync(oldUser);
        return ConvertError(deleteResult);
    }

    private static Option<ErrorOmd[]> ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? null
            : result.Errors.Select(x => new ErrorOmd { Code = x.Code, Name = x.Description }).ToArray();
    }
}
