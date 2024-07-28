﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebInstallers;

namespace CarcassDom.Installers;

public class CarcassDomInstaller : IInstaller
{
    public int InstallPriority => 30;
    public int ServiceUsePriority => 30;

    public void InstallServices(WebApplicationBuilder builder, bool debugMode, string[] args,
        Dictionary<string, string> parameters)
    {
        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(InstallServices)} Started");

        builder.Services.AddSingleton<FilterSortManager>();

        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(InstallServices)} Finished");
    }

    public void UseServices(WebApplication app, bool debugMode)
    {
    }
}