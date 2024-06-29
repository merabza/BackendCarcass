using LanguageExt;
using SystemToolsShared.Errors;

namespace CarcassDataSeeding;

public interface IDataSeeder
{
    Option<Err[]> Create(bool checkOnly);
}