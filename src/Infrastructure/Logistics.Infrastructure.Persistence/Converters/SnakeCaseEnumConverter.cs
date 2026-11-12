using System.Collections.Frozen;
using System.Text.Json;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Logistics.Infrastructure.Persistence.Converters;

/// <summary>Stores enums as snake_case strings; reading also accepts any case and numeric values.</summary>
public class SnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    /// <summary>Spellings existing rows keep, where a capital after a digit was not split.</summary>
    private static readonly FrozenDictionary<Enum, string> LegacyNames = new Dictionary<Enum, string>
    {
        [LicenseClass.EUC1E] = "euc1e"
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<TEnum, string> EnumToSnake =
        Enum.GetValues<TEnum>().Distinct().ToFrozenDictionary(v => v, ConvertName);

    private static readonly FrozenDictionary<string, TEnum> SnakeToEnum =
        EnumToSnake.ToFrozenDictionary(p => p.Value, p => p.Key, StringComparer.OrdinalIgnoreCase);

    public SnakeCaseEnumConverter()
        : base(v => ToSnakeCase(v), v => FromSnakeCase(v))
    {
    }

    private static string ConvertName(TEnum value) =>
        LegacyNames.GetValueOrDefault(value) ?? JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString());

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
