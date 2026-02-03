using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Logistics.Domain.Primitives.Enums;

public static partial class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        var type = enumValue.GetType();
        var fieldInfo = type.GetField(enumValue.ToString());

        if (fieldInfo is null)
        {
            return Humanize(enumValue.ToString());
        }

        var descAttribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;

        // Return the description if exists; otherwise, humanize the enum name
        return descAttribute?.Description ?? Humanize(enumValue.ToString());
    }

    /// <summary>
    /// Converts PascalCase to human-readable string.
    /// Examples:
    ///   "PickedUp" -> "Picked Up"
    ///   "OnDutyNotDriving" -> "On Duty Not Driving"
    /// </summary>
    private static string Humanize(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
        {
            return pascalCase;
        }

        return HumanizeRegex().Replace(pascalCase, "$1 $2");
    }

    [GeneratedRegex(@"([a-z])([A-Z])")]
    private static partial Regex HumanizeRegex();
}
