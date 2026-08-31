using System;

namespace BackendCarcass.Application.MasterData;

public /*open*/ class ReturnValuesLoaderCreator : IReturnValuesLoaderCreator
{
    protected readonly IServiceProvider Services;

    public ReturnValuesLoaderCreator(IServiceProvider services)
    {
        Services = services;
    }

    public virtual IReturnValuesLoader? CreateReturnValuesLoaderLoader(string tableName)
    {
        return null;
    }
}
