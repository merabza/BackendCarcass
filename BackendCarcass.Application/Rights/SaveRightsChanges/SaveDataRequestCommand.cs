using System.Collections.Generic;
using BackendCarcass.Rights.Models;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.Rights.SaveRightsChanges;

public sealed class SaveDataRequestCommand : ICommand<bool>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SaveDataRequestCommand(List<RightsChangeModel> changesForSave)
    {
        ChangesForSave = changesForSave;
    }

    public List<RightsChangeModel> ChangesForSave { get; }
}