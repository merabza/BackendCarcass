using MessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class DeleteCurrentUserCommandRequest : ICommand
{
    public string? UserName { get; set; }
}