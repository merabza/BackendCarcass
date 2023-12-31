﻿using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class ChildrenTreeDataCommandRequest : ICommand<List<DataTypeModel>>
{
    public ChildrenTreeDataCommandRequest(HttpRequest httpRequest, string dataTypeKey, ERightsEditorViewStyle viewStyle)
    {
        HttpRequest = httpRequest;
        ViewStyle = viewStyle;
        this.dataTypeKey = dataTypeKey;
    }

    public HttpRequest HttpRequest { get; set; }

    public ERightsEditorViewStyle ViewStyle { get; set; }
    public string dataTypeKey { get; set; }
}