using CarcassRepositories.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class MainMenuQueryRequest : IQuery<MainMenuModel>;