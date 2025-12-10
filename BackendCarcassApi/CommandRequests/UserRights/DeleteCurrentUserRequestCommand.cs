using MediatRMessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class DeleteCurrentUserRequestCommand : ICommand
{
    public string? UserName { get; set; }
}