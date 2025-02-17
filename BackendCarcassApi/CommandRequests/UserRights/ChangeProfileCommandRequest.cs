using MessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class ChangeProfileCommandRequest : ICommand
{
    public int Userid { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}