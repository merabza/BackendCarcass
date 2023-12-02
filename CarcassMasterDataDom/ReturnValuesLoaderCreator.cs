using System;
using CarcassMasterDataDom.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CarcassMasterDataDom;

public class ReturnValuesLoaderCreator : IReturnValuesLoaderCreator
{
    protected readonly IServiceProvider Services;

    public ReturnValuesLoaderCreator(IServiceProvider services)
    {
        Services = services;
    }

    public virtual IReturnValuesLoader CreateReturnValuesLoaderLoader(DataTypeModelForRvs dt)
    {
        var scope = Services.CreateScope();

        return new MasterDataReturnValuesLoader(dt, scope.ServiceProvider.GetRequiredService<IReturnValuesRepository>());
    }
}