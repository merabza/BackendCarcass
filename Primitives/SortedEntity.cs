namespace BackendCarcass.Domain.Primitives;

public abstract class SortedEntity<TId> : Entity<TId> where TId : notnull
{
    protected SortedEntity(TId id, int sortId) : base(id)
    {
        SortId = sortId;
    }

    public int SortId { get; }
}
