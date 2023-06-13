using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace ServerCarcassData;

public interface ICrudRepository
{
    IDbContextTransaction Transaction();

    Task SaveChangesAsync();

    //int SaveChanges();
    bool CheckUserAppClaimRight(string userName, string appClaimName);
    bool CheckUserToUserRight(string userName1, string userName2);
}