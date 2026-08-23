using BackendCarcass.Rights.Models;
using BackendCarcassDomain.Entities.ManyToManyJoins;
using BackendCarcassDomain.Entities.Users;

namespace BackendCarcass.Mappers;

public static class CarcassMainMappers
{
    public static UserModel AdaptTo(this User user)
    {
        return new UserModel(user.UsrId, user.NormalizedUserName, user.FullName);
    }

    public static ManyToManyJoinModel AdaptTo(this ManyToManyJoin mmj)
    {
        return new ManyToManyJoinModel(mmj.PtId, mmj.PKey, mmj.CtId, mmj.CKey);
    }
}
