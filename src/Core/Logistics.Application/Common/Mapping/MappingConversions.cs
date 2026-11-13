using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Common.Mapping;

/// <summary>Referenced by the mappers through <c>Use = nameof(...)</c>.</summary>
public static class MappingConversions
{
    public static string Describe(Enum value) => value.GetDescription();

    public static string DefaultToEmpty(string? value) => value ?? string.Empty;

    public static bool HasText(string? value) => !string.IsNullOrEmpty(value);
}
