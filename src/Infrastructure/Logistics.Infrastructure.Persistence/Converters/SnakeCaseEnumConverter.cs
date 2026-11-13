using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Logistics.Infrastructure.Persistence.Converters;

/// <summary>Stores enums as snake_case strings; reading also accepts any case and numeric values.</summary>
public class SnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    private static readonly FrozenDictionary<TEnum, string> EnumToSnake =
        Enum.GetValues<TEnum>().Distinct().ToFrozenDictionary(v => v, ConvertName);

    private static readonly FrozenDictionary<string, TEnum> SnakeToEnum =
        EnumToSnake.ToFrozenDictionary(p => p.Value, p => p.Key, StringComparer.OrdinalIgnoreCase);

    public SnakeCaseEnumConverter()
        : base(v => ToSnakeCase(v), v => FromSnakeCase(v))
    {
    }

    /// <summary>An <see cref="EnumMemberAttribute" /> value wins, so legacy spellings in existing rows still read.</summary>
    private static string ConvertName(TEnum value)
    {
        var name = value.ToString();
        var storedName = typeof(TEnum).GetField(name)?.GetCustomAttribute<EnumMemberAttribute>()?.Value;
        return storedName ?? JsonNamingPolicy.SnakeCaseLower.ConvertName(name);
    }

    private static string ToSnakeCase(TEnum value)
    {
        if (EnumToSnake.TryGetValue(value, out var name))
        {
            return name;
        }

        // Flag combinations format as "A, B". Stored rows separate the names with ", _".
        var parts = value.ToString().Split(", ");
        return string.Join(", _", parts.Select(JsonNamingPolicy.SnakeCaseLower.ConvertName));
    }

    private static TEnum FromSnakeCase(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        if (SnakeToEnum.TryGetValue(value, out var e))
        {
            return e;
        }

        return Enum.Parse<TEnum>(value.Replace("_", string.Empty), ignoreCase: true);
    }
}
