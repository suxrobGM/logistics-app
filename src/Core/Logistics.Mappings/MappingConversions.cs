using Logistics.Domain.Primitives.Enums;

namespace Logistics.Mappings;

/// <summary>
/// Conversions the Mapperly mappers reference through <c>Use = nameof(@MappingConversions.X)</c>.
/// </summary>
public static class MappingConversions
{
    public static string Describe(Enum value) => value.GetDescription();

    public static string DefaultToEmpty(string? value) => value ?? string.Empty;

    public static bool HasText(string? value) => !string.IsNullOrEmpty(value);
}
