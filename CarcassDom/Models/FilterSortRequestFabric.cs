using System;
using System.Text;
using System.Text.Json;
using CarcassMasterDataDom.Models;

namespace CarcassDom.Models;

public static class FilterSortRequestFabric
{
    public static FilterSortRequest? Create(string strFilterSortRequest)
    {
        var data = Convert.FromBase64String(strFilterSortRequest);
        var decodedString = Encoding.UTF8.GetString(data);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        return JsonSerializer.Deserialize<FilterSortRequest>(decodedString, options);
    }
}