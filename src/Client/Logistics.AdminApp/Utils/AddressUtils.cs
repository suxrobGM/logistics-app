using Logistics.Shared.Models;

namespace Logistics.AdminApp.Utils;

public static class AddressUtils
{
    public static string ConvertToString(AddressDto? address)
    {
        if (address is null)
        {
            return string.Empty;
        }
        
        if (!string.IsNullOrEmpty(address.Line2))
        {
            return $"{address.Line1}, {address.Line2}, {address.City}, {address.Region} {address.ZipCode}";
        }
        
        return $"{address.Line1}, {address.City}, {address.Region} {address.ZipCode}";
    }
}
