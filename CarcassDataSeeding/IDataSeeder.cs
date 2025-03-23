namespace CarcassDataSeeding;

public interface IDataSeeder
{
    bool Create(bool checkOnly);
}