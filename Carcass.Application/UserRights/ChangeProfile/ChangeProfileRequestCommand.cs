using MediatRMessagingAbstractions;

namespace Carcass.Application.UserRights.ChangeProfile;

public sealed class ChangeProfileRequestCommand : ICommand
{
    public int Userid { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}