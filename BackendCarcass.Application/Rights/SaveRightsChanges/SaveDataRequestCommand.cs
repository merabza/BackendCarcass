using System.Collections.Generic;
using BackendCarcass.Application.Rights.Models;
using SystemTools.Application.Abstractions.Messaging;

//using RightsChangeModel = BackendCarcass.Application.Rights.Models.RightsChangeModel;

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
