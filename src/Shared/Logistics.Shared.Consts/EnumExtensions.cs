using System.ComponentModel;

namespace Logistics.Shared.Consts;

public static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        var type = enumValue.GetType();
        var fieldInfo = type.GetField(enumValue.ToString());

        if (fieldInfo is null)
        {
            return enumValue.ToString();
        }
        
        var descAttribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
            
        // Return the description, if it exists; otherwise, return the enum name
        return descAttribute == null ? enumValue.ToString() : descAttribute.Description;
    }
}
