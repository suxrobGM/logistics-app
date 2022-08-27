using System.Reflection;

namespace Logistics.Application.Shared;

public static class TypeExtensions
{
    public static bool HasProperty(this Type type, string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return false;
        
        var propertiesInfo = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return propertiesInfo.Any(propertyInfo => propertyInfo.Name == propertyName);
    }
}