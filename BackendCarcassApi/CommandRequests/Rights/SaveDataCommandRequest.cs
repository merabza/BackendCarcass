using System.Collections.Generic;
using CarcassDom.Models;
using MessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class SaveDataCommandRequest : ICommand<bool>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SaveDataCommandRequest(List<RightsChangeModel> changesForSave)
    {
        ChangesForSave = changesForSave;
    }

    public List<RightsChangeModel> ChangesForSave { get; }
}