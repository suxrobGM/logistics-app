using System.Reflection;
using Logistics.Application.Abstractions.Agents;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Every tool in this assembly, discovered once at startup from the classes themselves - so there is
/// no list anywhere that can fall out of date.
/// </summary>
internal static class AgentToolCatalog
{
    private static readonly (Type Type, AgentToolDefinition Definition)[] Entries = Discover();

    /// <summary>Reads first, writes last, as the system prompts describe them.</summary>
    public static IReadOnlyList<AgentToolDefinition> Definitions { get; } =
        [.. Entries.Select(e => e.Definition)];

    public static IReadOnlyList<Type> ToolTypes { get; } = [.. Entries.Select(e => e.Type)];

    private static readonly Dictionary<string, Type> TypesByName =
        Entries.ToDictionary(e => e.Definition.Name, e => e.Type);

    private static readonly Dictionary<string, AgentToolDefinition> DefinitionsByName =
        Entries.ToDictionary(e => e.Definition.Name, e => e.Definition);

    private static readonly Dictionary<string, IReadOnlyList<(AgentEntityKind Kind, string Key)>> EntityIdsByName =
        Entries.ToDictionary(e => e.Definition.Name, e => AgentToolJson.EntityIdsIn(InputTypeOf(e.Type)));

    public static Type? ImplementationFor(string name) => TypesByName.GetValueOrDefault(name);

    public static AgentToolDefinition? DefinitionFor(string name) => DefinitionsByName.GetValueOrDefault(name);

    /// <summary>
    /// The entity links a call to this tool can fill on its decision row. Empty for an unknown
    /// name, so a hallucinated tool records nothing rather than throwing.
    /// </summary>
    public static IReadOnlyList<(AgentEntityKind Kind, string Key)> EntityIdsFor(string name) =>
        EntityIdsByName.GetValueOrDefault(name, []);

    private static (Type Type, AgentToolDefinition Definition)[] Discover() =>
        [.. typeof(AgentToolCatalog).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(IAgentTool)))
            .Select(t => (Type: t, Definition: DefinitionOf(t)))
            // Reflection order is not guaranteed, and the catalogue sits in the cached prompt
            // prefix - it has to come out the same way every time.
            .OrderBy(e => e.Definition.IsWrite)
            .ThenBy(e => e.Definition.Name, StringComparer.Ordinal)];

    private static AgentToolDefinition DefinitionOf(Type toolType)
    {
        var property = toolType.GetProperty(
            nameof(IAgentToolMetadata.Definition),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (property?.GetValue(null) is not AgentToolDefinition definition)
        {
            throw new InvalidOperationException(
                $"{toolType.Name} implements IAgentTool but declares no catalogue entry. "
                + "Implement IAgentToolMetadata with a public static Definition property.");
        }

        return definition with { InputSchema = AgentToolJson.SchemaFor(InputTypeOf(toolType)) };
    }

    private static Type InputTypeOf(Type toolType)
    {
        for (var type = toolType; type is not null; type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AgentTool<>))
                return type.GetGenericArguments()[0];
        }

        throw new InvalidOperationException(
            $"{toolType.Name} must derive from AgentTool<TInput> so its input schema can be generated.");
    }
}
