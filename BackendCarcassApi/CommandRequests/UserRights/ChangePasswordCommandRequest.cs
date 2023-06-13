using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class ChangePasswordCommandRequest : ICommand
{
    public ChangePasswordCommandRequest(int userid, string? userName, string? oldPassword, string? newPassword,
        string? newPasswordConfirm, HttpRequest httpRequest)
    {
        Userid = userid;
        UserName = userName;
        OldPassword = oldPassword;
        NewPassword = newPassword;
        NewPasswordConfirm = newPasswordConfirm;
        HttpRequest = httpRequest;
    }

    public int Userid { get; set; }
    public string? UserName { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? NewPasswordConfirm { get; set; }
    public HttpRequest HttpRequest { get; set; }
}