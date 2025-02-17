using CarcassRepositories.Models;
using MessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class MainMenuQueryRequest : IQuery<MainMenuModel>;