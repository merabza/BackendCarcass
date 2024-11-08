using System.Collections.Generic;
using CarcassDom.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class SaveDataCommandRequest : ICommand<bool>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SaveDataCommandRequest(HttpRequest httpRequest, List<RightsChangeModel> changesForSave)
    {
        HttpRequest = httpRequest;
        ChangesForSave = changesForSave;
    }

    public HttpRequest HttpRequest { get; set; }
    public List<RightsChangeModel> ChangesForSave { get; }
}