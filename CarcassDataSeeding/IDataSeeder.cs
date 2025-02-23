using System.Collections.Generic;
using LanguageExt;
using SystemToolsShared.Errors;

namespace CarcassDataSeeding;

public interface IDataSeeder
{
    Option<IEnumerable<Err>> Create(bool checkOnly);
}