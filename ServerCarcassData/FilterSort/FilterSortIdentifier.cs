namespace ServerCarcassData.FilterSort;

public sealed class FilterSortIdentifier // : IEqualityComparer<FilterSortIdentifier>
{
    public FilterSortIdentifier(int userSerialNumber, int tabWindowId, string tableName)
    {
        UserSerialNumber = userSerialNumber;
        TabWindowId = tabWindowId;
        TableName = tableName;
    }

    public int UserSerialNumber { get; set; }
    public int TabWindowId { get; set; }
    public string TableName { get; set; }

    //public bool Equals(FilterSortIdentifier x, FilterSortIdentifier y)
    //{
    //  //First check if both object reference are equal then return true
    //  if (ReferenceEquals(x, y))
    //  {
    //    return true;
    //  }
    //  //If either one of the object reference is null, return false
    //  if (x is null || y is null)
    //  {
    //    return false;
    //  }
    //  //Comparing all the properties one by one
    //  return x.UserSerialNumber == y.UserSerialNumber && x.TabWindowId == y.TabWindowId &&
    //         x.TableName == y.TableName;
    //}

    //public int GetHashCode(FilterSortIdentifier obj)
    //{
    //  return obj.UserSerialNumber.GetHashCode() ^ obj.TabWindowId.GetHashCode() ^ obj.TableName.GetHashCode();
    //}
}