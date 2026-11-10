using System;

namespace BackendCarcass.Api.Tests.Sso.Infrastructure;

//დრო, რომელსაც ტესტი თვითონ აგრძელებს (წვდომის ტოკენის ვადის გასასვლელად)
internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public ManualTimeProvider(DateTimeOffset now)
    {
        _now = now;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _now;
    }

    public void Advance(TimeSpan by)
    {
        _now += by;
    }
}
