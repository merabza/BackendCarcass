using System;
using System.Text;
using System.Text.Json;
using System.Web;
using BackendCarcass.LibCrud.Models;

namespace BackendCarcass.FilterSort.Models;

public static class FilterSortRequestFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static FilterSortRequest? Create(string strFilterSortRequest)
    {
        byte[] data = Convert.FromBase64String(strFilterSortRequest);
        string decodedString = Encoding.UTF8.GetString(data);

        return JsonSerializer.Deserialize<FilterSortRequest>(HttpUtility.UrlDecode(decodedString), JsonOptions);
    }
}
