using System.Collections.Generic;
using CarcassRepositories.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.CommandRequests.Rights;

public sealed class SaveDataCommandRequest : ICommand<bool>
{
    public SaveDataCommandRequest(HttpRequest httpRequest, List<RightsChangeModel> changesForSave)
    {
        HttpRequest = httpRequest;
        ChangesForSave = changesForSave;
    }

    public HttpRequest HttpRequest { get; set; }
    public List<RightsChangeModel> ChangesForSave { get; }
}