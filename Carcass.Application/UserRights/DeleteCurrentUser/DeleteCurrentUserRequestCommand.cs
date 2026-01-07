using MediatRMessagingAbstractions;

namespace Carcass.Application.UserRights.DeleteCurrentUser;

public sealed class DeleteCurrentUserRequestCommand : ICommand
{
    public string? UserName { get; set; }
}