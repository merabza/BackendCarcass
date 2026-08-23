using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.UserRights.DeleteCurrentUser;

public sealed class DeleteCurrentUserRequestCommand : ICommandOmd
{
    public string? UserName { get; set; }
}
