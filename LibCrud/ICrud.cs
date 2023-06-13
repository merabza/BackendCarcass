using System.Threading.Tasks;
using LanguageExt;
using OneOf;
using SystemToolsShared;

namespace LibCrud;

public interface ICrud
{
    Task<OneOf<ICrudData, Err[]>> GetOne(int id);
    Task<OneOf<ICrudData, Err[]>> Create(ICrudData crudDataForCreate);
    Task<Option<Err[]>> Update(int id, ICrudData crudDataNewVersion);
    Task<Option<Err[]>> Delete(int id);
}