using CarcassDb.Models;
using CarcassDom.Models;

namespace CarcassMappers;

public static class CarcassMainMappers
{
    public static UserModel AdaptTo(this User user)
    {
        return new UserModel(user.UsrId, user.NormalizedUserName, user.FullName);
    }
}