﻿using System;

namespace CarcassMasterDataDom.Models;

public sealed class MenuToCrudTypeDomModel : IDataType
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MenuToCrudTypeDomModel(int dtctId, string dtctKey, string dtctName)
    {
        MnctId = dtctId;
        MnctKey = dtctKey;
        MnctName = dtctName;
    }

    public int MnctId { get; set; }
    public string MnctKey { get; init; }
    public string MnctName { get; init; }

    public int Id
    {
        get => MnctId;
        set => MnctId = value;
    }

    public string Key => MnctKey;
    public string Name => MnctName;
    public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        throw new NotImplementedException();
    }

    public dynamic EditFields()
    {
        return new { MnctId, MnctKey, MnctName };
    }
}