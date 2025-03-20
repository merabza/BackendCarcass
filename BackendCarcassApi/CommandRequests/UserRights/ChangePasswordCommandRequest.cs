using MediatRMessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class ChangePasswordCommandRequest : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangePasswordCommandRequest(int userid, string? userName, string? oldPassword, string? newPassword,
        string? newPasswordConfirm)
    {
        Userid = userid;
        UserName = userName;
        OldPassword = oldPassword;
        NewPassword = newPassword;
        NewPasswordConfirm = newPasswordConfirm;
    }

    public int Userid { get; set; }
    public string? UserName { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? NewPasswordConfirm { get; set; }
}