using Radzen;

namespace Logistics.AdminApp.Extensions;

public static class LoadDataArgsExtensions
{
    public static int GetPageNumber(this LoadDataArgs loadDataArgs)
    {
        var top = loadDataArgs.Top ?? 10;
        var skip = loadDataArgs.Skip ?? 0;
        var page = (int)Math.Ceiling((skip + 1) / (double)top);
        return page;
    }

    public static int GetPageSize(this LoadDataArgs loadDataArgs)
    {
        return loadDataArgs.Top ?? 10;
    }

    public static string GetOrderBy(this LoadDataArgs loadDataArgs)
    {
        if (string.IsNullOrEmpty(loadDataArgs.OrderBy))
        {
            return string.Empty;
        }

        var splitStr = loadDataArgs.OrderBy.Split(' ');

        switch (splitStr.Length)
        {
            case 1:
                return splitStr[0];
            case 0:
                return string.Empty;
            default:
                {
                    var sortProperty = splitStr[0];
                    var orderSpecifier = splitStr[1];
                    return orderSpecifier == "desc" ? $"-{sortProperty}" : sortProperty;
                }
        }
    }
}
