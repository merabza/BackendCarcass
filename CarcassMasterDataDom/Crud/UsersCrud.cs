using CarcassMasterDataDom.Models;
using LanguageExt;
using LibCrud;
using LibCrud.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using RepositoriesDom;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom.Crud;

public class UsersCrud : CrudBase, IMasterDataLoader
{
    private readonly UserManager<AppUser> _userManager;
    private AppUser? _justCreated;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UsersCrud(ILogger logger, UserManager<AppUser> userManager, IAbstractRepository absRepo) : base(logger,
        absRepo)
    {
        _userManager = userManager;
    }

    protected override int JustCreatedId => _justCreated?.Id ?? 0;

    public async Task<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(users
            .Where(x => x.UserName is not null && x.Email is not null)
            .Select(x => new UserCrudData(x.UserName!, x.FirstName, x.LastName, x.Email!)));
    }

    public override async Task<OneOf<TableRowsData, Err[]>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken)
    {
        var users = _userManager.Users;
        var (realOffset, count, rows) = await users.UseCustomSortFilterPagination(filterSortRequest,
            x => new UserCrudData(x.UserName!, x.FirstName, x.LastName, x.Email!), cancellationToken);

        return new TableRowsData(count, realOffset, rows.Select(s => s.EditFields()).ToList());
    }

    protected override async Task<OneOf<ICrudData, Err[]>> GetOneData(int id, CancellationToken cancellationToken)
    {
        var appUser = await _userManager.FindByIdAsync(id.ToString());
        if (appUser?.UserName is not null && appUser.Email is not null)
            return new UserCrudData(appUser.UserName, appUser.FirstName, appUser.LastName, appUser.Email);
        return new[] { MasterDataApiErrors.CannotFindUser };
    }

    protected override async Task<Option<Err[]>> CreateData(ICrudData crudDataForCreate,
        CancellationToken cancellationToken)
    {
        var user = (UserCrudData)crudDataForCreate;
        AppUser appUser = new(user.UserName, user.FirstName, user.LastName)
        {
            Email = user.Email
        };

        //შევქმნათ როლი
        var createResult = await _userManager.CreateAsync(appUser);
        if (!createResult.Succeeded)
            return ConvertError(createResult);
        _justCreated = appUser;
        return null;
    }

    protected override async Task<Option<Err[]>> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken)
    {
        var oldUser = await _userManager.FindByIdAsync(id.ToString());
        if (oldUser is null)
            return new[] { MasterDataApiErrors.CannotFindUser };

        var user = (UserCrudData)crudDataNewVersion;
        oldUser.UserName = user.UserName;
        oldUser.Email = user.Email;
        oldUser.FirstName = user.FirstName;
        oldUser.LastName = user.LastName;

        var updateResult = await _userManager.UpdateAsync(oldUser);
        if (!updateResult.Succeeded)
            return ConvertError(updateResult);

        if (oldUser.UserName != user.UserName)
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(oldUser, user.UserName);
            if (!setUserNameResult.Succeeded)
                return ConvertError(setUserNameResult);
        }

        if (oldUser.Email == user.Email)
            return null;
        var setEmailResult = await _userManager.SetEmailAsync(oldUser, user.Email);
        return ConvertError(setEmailResult);
    }

    protected override async Task<Option<Err[]>> DeleteData(int id, CancellationToken cancellationToken)
    {
        var oldUser = await _userManager.FindByIdAsync(id.ToString());
        if (oldUser is null)
            return new[] { MasterDataApiErrors.CannotFindUser };
        var deleteResult = await _userManager.DeleteAsync(oldUser);
        return ConvertError(deleteResult);
    }

    private static Option<Err[]> ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? null
            : result.Errors.Select(x => new Err { ErrorCode = x.Code, ErrorMessage = x.Description }).ToArray();
    }
}