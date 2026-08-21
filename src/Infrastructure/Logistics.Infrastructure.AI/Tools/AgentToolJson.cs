using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// The schema the model is shown and the binder that reads its reply, both derived from a tool's
/// input type - so a renamed property cannot appear in one and not the other.
/// </summary>
internal static class AgentToolJson
{
    /// <summary>Lenient: models emit quoted numbers, unquoted text and keys in the wrong case.</summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter(), new UtcDateTimeConverter(), new ScalarToStringConverter() },
        // Serialization would default this in; the schema exporter refuses to run without it.
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private static readonly JsonSchemaExporterOptions ExporterOptions = new()
    {
        // Otherwise every reference-typed property exports as nullable, reading to the model as
        // "you may send null" for properties that are in fact required.
        TreatNullObliviousAsNonNullable = true,
        TransformSchemaNode = Describe
    };

    /// <summary>
    /// A tool's input schema: snake_case names, <c>[Description]</c> as the description, and
    /// <c>required</c> members as the required list.
    /// </summary>
    public static JsonNode SchemaFor(Type inputType)
    {
        var schema = Options.GetJsonSchemaAsNode(inputType, ExporterOptions);

        // A tool with no arguments exports without a properties key, which some providers reject.
        var root = schema as JsonObject ?? [];
        root["type"] = "object";
        root["properties"] ??= new JsonObject();
        return root;
    }

    /// <summary>
    /// Reads the model's arguments into <typeparamref name="TInput"/>. Failures come back as text
    /// rather than exceptions: the model can fix an argument next turn if it is told which one.
    /// </summary>
    public static bool TryBind<TInput>(
        JsonNode input,
        [NotNullWhen(true)] out TInput? value,
        [NotNullWhen(false)] out string? error)
        where TInput : class
    {
        value = null;

        var missing = MissingRequiredKeys(typeof(TInput), input);
        if (missing.Count > 0)
        {
            error = $"Missing required input: {string.Join(", ", missing)}";
            return false;
        }

        try
        {
            value = input.Deserialize<TInput>(Options);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or FormatException)
        {
            var path = (ex as JsonException)?.Path;
            error = path is null
                ? "Tool input could not be read - check the argument types against the schema."
                : $"Invalid value at {path} - check its type against the schema.";
            return false;
        }

        if (value is null)
        {
            error = "Tool input must be a JSON object.";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Checked before deserializing, because the exception for a missing <c>required</c> member does
    /// not name it in terms the model can act on.
    /// </summary>
    private static List<string> MissingRequiredKeys(Type inputType, JsonNode input)
    {
        // Case-insensitive to match the binder, but not underscore-insensitive.
        var present = input is JsonObject obj
            ? obj.Select(p => p.Key).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return [.. Options.GetTypeInfo(inputType).Properties
            .Where(p => p.IsRequired && !present.Contains(p.Name))
            .Select(p => p.Name)];
    }

    private static JsonNode Describe(JsonSchemaExporterContext context, JsonNode schema)
    {
        var type = Nullable.GetUnderlyingType(context.TypeInfo.Type) ?? context.TypeInfo.Type;

        var node = ConverterSchema(type) ?? schema as JsonObject ?? [];

        CollapseTypeUnion(node);

        if (node["enum"] is JsonArray values)
        {
            // An optional enum lists null among its values; omitting the property already says that.
            foreach (var nullValue in values.Where(v => v is null).ToList())
                values.Remove(nullValue);

            node["type"] ??= "string";
        }

        var attributes = context.PropertyInfo?.AttributeProvider;
        if (attributes?.GetCustomAttributes(typeof(DescriptionAttribute), inherit: true).FirstOrDefault()
            is DescriptionAttribute description)
        {
            node["description"] = description.Description;
        }

        return node;
    }

    /// <summary>The exporter cannot see through a custom converter - it exports "any" instead.</summary>
    private static JsonObject? ConverterSchema(Type type) => type switch
    {
        _ when type == typeof(DateTime) => new JsonObject { ["type"] = "string", ["format"] = "date-time" },
        _ when type == typeof(string) => new JsonObject { ["type"] = "string" },
        _ => null
    };

    /// <summary>
    /// The lenient reader shows up in the schema: an optional <c>int?</c> exports as
    /// <c>["string", "integer", "null"]</c>. State the one type meant; keep the leniency here.
    /// </summary>
    private static void CollapseTypeUnion(JsonObject node)
    {
        if (node["type"] is not JsonArray union)
            return;

        var names = union.Select(t => t!.GetValue<string>()).Where(t => t != "null").ToList();
        var preferred = names.FirstOrDefault(t => t != "string") ?? names.FirstOrDefault();
        if (preferred is null)
            return;

        node["type"] = preferred;

        if (preferred is "integer" or "number")
            node.Remove("pattern");
    }
}
