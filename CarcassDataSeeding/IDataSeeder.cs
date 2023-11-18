using LanguageExt;
using SystemToolsShared;

namespace CarcassDataSeeding;

public interface IDataSeeder
{
    Option<Err[]> Create(bool checkOnly);
}