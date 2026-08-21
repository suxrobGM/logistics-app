using System.Text.Json.Nodes;
using Logistics.Infrastructure.AI;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

/// <summary>
/// The catalogue is built from the tool classes, so what is left to check is what the compiler
/// cannot: that the scan finds them, that names are unique and shaped right, and that each schema
/// is generated rather than empty.
/// </summary>
public class AgentToolCatalogTests
{
    [Fact]
    public void Catalog_DiscoversEveryTool()
    {
        // Discovery throws on a tool with no definition or a duplicate name, so reading the
        // catalogue is the assertion; the count guards a scan predicate that matches nothing.
        Assert.True(AgentToolCatalog.Definitions.Count > 25);
        Assert.Equal(AgentToolCatalog.ToolTypes.Count, AgentToolCatalog.Definitions.Count);
    }

    [Fact]
    public void EveryTool_IsRegisteredInDi()
    {
        var services = new ServiceCollection();
        services.AddAIInfrastructure(new ConfigurationBuilder().Build());
        var registered = services.Select(d => d.ServiceType).ToHashSet();

        var missing = AgentToolCatalog.ToolTypes.Where(t => !registered.Contains(t)).Select(t => t.Name).ToList();

        Assert.True(missing.Count == 0,
            $"Tools the executor cannot resolve: {string.Join(", ", missing)}");
    }

    [Fact]
    public void ToolNames_AreSnakeCase()
    {
        Assert.All(AgentToolCatalog.Definitions, d => Assert.Matches("^[a-z][a-z0-9_]*$", d.Name));
    }

    [Fact]
    public void ClassNames_MirrorToolNames()
    {
        // A transcript names the tool; the reader has to be able to find the class from it.
        var mismatched = AgentToolCatalog.ToolTypes
            .Zip(AgentToolCatalog.Definitions)
            .Where(pair => !pair.First.Name.Equals(ExpectedClassName(pair.Second.Name), StringComparison.OrdinalIgnoreCase))
            .Select(pair => $"{pair.First.Name} declares '{pair.Second.Name}'")
            .ToList();

        Assert.True(mismatched.Count == 0, string.Join(", ", mismatched));
    }

    [Fact]
    public void EveryTool_HasAGeneratedObjectSchema()
    {
        Assert.All(AgentToolCatalog.Definitions, definition =>
        {
            Assert.False(string.IsNullOrWhiteSpace(definition.Description));
            var schema = Assert.IsType<JsonObject>(definition.InputSchema);
            Assert.Equal("object", schema["type"]!.GetValue<string>());
            Assert.NotNull(schema["properties"]);
        });
    }

    [Fact]
    public void ReadTools_ComeBeforeWriteTools()
    {
        var firstWrite = AgentToolCatalog.Definitions.ToList().FindIndex(d => d.IsWrite);
        var lastRead = AgentToolCatalog.Definitions.ToList().FindLastIndex(d => !d.IsWrite);

        Assert.True(firstWrite > lastRead);
    }

    /// <summary>
    /// A lenient converter blinds the schema exporter for its type, and the property then publishes
    /// as "anything". Registering one without teaching <c>AgentToolJson.ConverterSchema</c> about it
    /// otherwise fails silently: the model just stops being told what the argument is.
    /// </summary>
    [Fact]
    public void EveryInputProperty_StatesItsType()
    {
        var untyped = AgentToolCatalog.Definitions
            .SelectMany(d => ((JsonObject)d.InputSchema["properties"]!)
                .Select(p => (Tool: d.Name, Property: p.Key, Schema: p.Value as JsonObject)))
            .Where(p => p.Schema?["type"] is null && p.Schema?["enum"] is null)
            .Select(p => $"{p.Tool}.{p.Property}")
            .ToList();

        Assert.True(untyped.Count == 0, $"Properties with no type: {string.Join(", ", untyped)}");
    }

    /// <summary>
    /// An unmarked entity id costs nothing at the call and everything afterwards: the decision row
    /// records no link, so the load or truck the agent acted on cannot be traced back to it, and
    /// nothing anywhere fails.
    /// </summary>
    [Fact]
    public void EntityIdInputs_AreMarkedForTheAuditTrail()
    {
        string[] linkKeys =
            ["load_id", "truck_id", "trip_id", "invoice_id", "customer_id", "negotiation_id"];

        var unmarked = AgentToolCatalog.Definitions
            .SelectMany(d => ((JsonObject)d.InputSchema["properties"]!)
                .Select(p => (Tool: d.Name, Key: p.Key)))
            .Where(p => linkKeys.Contains(p.Key))
            .Where(p => !AgentToolCatalog.EntityIdsFor(p.Tool).Any(e => e.Key == p.Key))
            .Select(p => $"{p.Tool}.{p.Key}")
            .ToList();

        Assert.True(unmarked.Count == 0,
            $"Entity ids with no [AgentEntityId]: {string.Join(", ", unmarked)}");
    }

    /// <summary>Compared case-insensitively: search_loadboard is SearchLoadBoardTool.</summary>
    private static string ExpectedClassName(string toolName) =>
        toolName.Replace("_", "") + "Tool";
}
