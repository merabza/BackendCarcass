using CarcassMasterDataDom.Models;

namespace CarcassMasterDataDom;

public interface IReturnValuesLoaderCreator
{
    IReturnValuesLoader CreateReturnValuesLoaderLoader(DataTypeModelForRvs dt);
}