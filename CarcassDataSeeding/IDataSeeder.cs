using System.Collections.Generic;

namespace CarcassDataSeeding;

public interface IDataSeeder
{
    (bool success, List<string> messages) Create(bool checkRecordsExists = true);
}