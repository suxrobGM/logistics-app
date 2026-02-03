using System.Collections.Frozen;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Logistics.Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core value converter that stores enum values as snake_case strings in the database.
/// E.g., LoadStatus.PickedUp -> "picked_up"
/// Uses cached dictionaries for O(1) lookups instead of computing on every call.
/// </summary>
public class SnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    private static readonly FrozenDictionary<TEnum, string> enumToSnake = BuildEnumToSnakeCache();
    private static readonly FrozenDictionary<string, TEnum> snakeToEnum = BuildSnakeToEnumCache();

    public SnakeCaseEnumConverter()
        : base(
            v => enumToSnake.GetValueOrDefault(v, ToSnakeCase(v.ToString())),
            v => snakeToEnum.GetValueOrDefault(v, ParseFallback(v)))
    {
    }

    private static FrozenDictionary<TEnum, string> BuildEnumToSnakeCache()
    {
        var values = Enum.GetValues<TEnum>();
        var dict = new Dictionary<TEnum, string>(values.Length);

        foreach (var value in values)
        {
            dict[value] = ToSnakeCase(value.ToString());
        }

        return dict.ToFrozenDictionary();
    }

    private static FrozenDictionary<string, TEnum> BuildSnakeToEnumCache()
    {
        var values = Enum.GetValues<TEnum>();
        var dict = new Dictionary<string, TEnum>(values.Length, StringComparer.OrdinalIgnoreCase);

        foreach (var value in values)
        {
            dict[ToSnakeCase(value.ToString())] = value;
        }

        return dict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static TEnum ParseFallback(string snakeCase)
    {
        // Fallback for values not in cache (shouldn't happen normally)
        var pascalCase = ToPascalCase(snakeCase);
        return Enum.Parse<TEnum>(pascalCase, ignoreCase: true);
    }

    private static string ToSnakeCase(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
        {
            return pascalCase;
        }

        var sb = new StringBuilder(pascalCase.Length + 4);
        for (var i = 0; i < pascalCase.Length; i++)
        {
            var c = pascalCase[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    sb.Append('_');
                }

                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static string ToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
        {
            return snakeCase;
        }

        var sb = new StringBuilder(snakeCase.Length);
        var capitalizeNext = true;

        foreach (var c in snakeCase)
        {
            if (c == '_')
            {
                capitalizeNext = true;
            }
            else if (capitalizeNext)
            {
                sb.Append(char.ToUpperInvariant(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }
}
