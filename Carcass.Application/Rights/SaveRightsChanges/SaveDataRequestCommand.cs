using System.Collections.Generic;
using CarcassDom.Models;
using MediatRMessagingAbstractions;

namespace Carcass.Application.Rights.SaveRightsChanges;

public sealed class SaveDataRequestCommand : ICommand<bool>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SaveDataRequestCommand(List<RightsChangeModel> changesForSave)
    {
        ChangesForSave = changesForSave;
    }

    public List<RightsChangeModel> ChangesForSave { get; }
}