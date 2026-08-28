using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.UserRights.DeleteCurrentUser;

public sealed class DeleteCurrentUserRequestCommand : ICommand
{
    public string? UserName { get; set; }
}
