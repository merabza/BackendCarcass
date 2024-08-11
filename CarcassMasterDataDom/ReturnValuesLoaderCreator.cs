using System;

namespace CarcassMasterDataDom;

public class ReturnValuesLoaderCreator : IReturnValuesLoaderCreator
{
    protected readonly IServiceProvider Services;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ReturnValuesLoaderCreator(IServiceProvider services)
    {
        Services = services;
    }

    public virtual IReturnValuesLoader? CreateReturnValuesLoaderLoader(string tableName)
    {
        return null;
    }
}