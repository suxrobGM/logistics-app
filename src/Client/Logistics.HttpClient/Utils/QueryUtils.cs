using System.Text;
using System.Web;

namespace Logistics.HttpClient.Utils;

internal static class QueryUtils
{
    public static string ParseQueryParam(string uriString, string paramKey)
    {
        try
        {
            return ParseQueryParam(new Uri(uriString.ToLower()), paramKey);
        }
        catch (UriFormatException)
        {
            return string.Empty;
        }
    }

    public static string ParseQueryParam(Uri uri, string paramKey)
    {
        var queryString = HttpUtility.ParseQueryString(uri.Query.ToLower());
        return queryString.Get(paramKey) ?? string.Empty;
    }

    public static string BuildQueryParameters(string uriString,
        IDictionary<string, string> queries)
    {
        if (queries.Count == 0)
            return uriString;

        if (uriString.EndsWith("/"))
            uriString = uriString.Substring(0, uriString.Length - 1);

        var andChar = string.Empty;
        var strBuilder = new StringBuilder(uriString);
        strBuilder.Append('?');

        foreach (var c in queries)
        {
            strBuilder.Append($"{andChar}{c.Key}={c.Value}");
            andChar = "&";
        }

        return strBuilder.ToString();
    }
}
