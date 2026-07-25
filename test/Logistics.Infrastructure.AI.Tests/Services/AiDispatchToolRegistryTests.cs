using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Services;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AiDispatchToolRegistryTests
{
    private readonly AiDispatchToolRegistry sut = new();

    /// <summary>A tenant with no gated features - the baseline every ungated tool must survive.</summary>
    private static readonly IReadOnlySet<TenantFeature> NoFeatures = new HashSet<TenantFeature>();

    private static IReadOnlySet<TenantFeature> With(params TenantFeature[] features) =>
        new HashSet<TenantFeature>(features);

    [Fact]
    public void GetToolDefinitions_ReturnsNonEmptyList()
    {
        var tools = sut.GetToolDefinitions(NoFeatures);

        Assert.NotEmpty(tools);
    }

    [Fact]
    public void GetToolDefinitions_AllToolsHaveNameAndDescription()
    {
        var tools = sut.GetToolDefinitions(NoFeatures);

        foreach (var tool in tools)
        {
            Assert.False(string.IsNullOrWhiteSpace(tool.Name), "Tool has empty name");
            Assert.False(string.IsNullOrWhiteSpace(tool.Description), $"Tool '{tool.Name}' has empty description");
            Assert.NotNull(tool.InputSchema);
        }
    }

    [Fact]
    public void GetToolDefinitions_ExcludesLoadBoardTools_ByDefault()
    {
        var tools = sut.GetToolDefinitions(NoFeatures);

        Assert.DoesNotContain(tools, t => t.Name == "search_loadboard");
        Assert.DoesNotContain(tools, t => t.Name == "book_loadboard_load");
    }

    [Fact]
    public void GetToolDefinitions_IncludesLoadBoardTools_WhenRequested()
    {
        var tools = sut.GetAllToolDefinitions();

        Assert.Contains(tools, t => t.Name == "search_loadboard");
        Assert.Contains(tools, t => t.Name == "book_loadboard_load");
    }

    [Fact]
    public void GetToolDefinitions_ContainsCoreReadTools()
    {
        var tools = sut.GetToolDefinitions(NoFeatures);
        var names = tools.Select(t => t.Name).ToHashSet();

        Assert.Contains("get_unassigned_loads", names);
        Assert.Contains("get_available_trucks", names);
        Assert.Contains("get_driver_hos_status", names);
        Assert.Contains("check_hos_feasibility", names);
        Assert.Contains("batch_check_hos_feasibility", names);
        Assert.Contains("calculate_distance", names);
    }

    /// <summary>
    /// The intermodal tools are plain reads - listing either in <c>WriteTools</c> would put them
    /// behind dispatcher approval and stall the agent.
    /// </summary>
    [Fact]
    public void GetToolDefinitions_IntermodalToolsAreReadsNotWrites()
    {
        var names = sut.GetToolDefinitions(With(TenantFeature.IntermodalContainers)).Select(t => t.Name).ToHashSet();

        Assert.Contains("get_container_status", names);
        Assert.Contains("get_terminal_info", names);
    }

    /// <summary>
    /// Their schemas cost tokens on every request, so a tenant without the feature must not get them.
    /// </summary>
    [Fact]
    public void GetToolDefinitions_WithoutIntermodalFeature_OmitsTheIntermodalTools()
    {
        var names = sut.GetToolDefinitions(NoFeatures).Select(t => t.Name).ToHashSet();

        Assert.DoesNotContain("get_container_status", names);
        Assert.DoesNotContain("get_terminal_info", names);
    }

    /// <summary>The gated groups are independent - one feature may not pull in another's tools.</summary>
    [Fact]
    public void GetToolDefinitions_GatedGroupsAreIndependent()
    {
        var loadBoardOnly = sut.GetToolDefinitions(With(TenantFeature.LoadBoard))
            .Select(t => t.Name).ToHashSet();
        Assert.Contains("search_loadboard", loadBoardOnly);
        Assert.DoesNotContain("get_container_status", loadBoardOnly);

        var intermodalOnly = sut.GetToolDefinitions(With(TenantFeature.IntermodalContainers))
            .Select(t => t.Name).ToHashSet();
        Assert.Contains("get_container_status", intermodalOnly);
        Assert.DoesNotContain("search_loadboard", intermodalOnly);
    }

    /// <summary>
    /// The gate is metadata on the definition, so the MCP surface - which publishes every tool and
    /// checks per call - reads the same value the schema filter does. Two copies of a tool-name list
    /// is how the old MCP copy silently lost `check_broker_credit`.
    /// </summary>
    [Fact]
    public void GetAllToolDefinitions_CarriesTheFeatureEachGatedToolNeeds()
    {
        var all = sut.GetAllToolDefinitions().ToDictionary(t => t.Name, t => t.RequiredFeature);

        Assert.Equal(TenantFeature.LoadBoard, all["search_loadboard"]);
        Assert.Equal(TenantFeature.LoadBoard, all["check_broker_credit"]);
        Assert.Equal(TenantFeature.LoadBoard, all["book_loadboard_load"]);
        Assert.Equal(TenantFeature.IntermodalContainers, all["get_container_status"]);
        Assert.Equal(TenantFeature.IntermodalContainers, all["get_terminal_info"]);
        Assert.Null(all["get_unassigned_loads"]);
    }

    /// <summary>Every gated tool must be reachable - a feature nobody grants is a dead tool.</summary>
    [Fact]
    public void GetToolDefinitions_WithEveryFeature_MatchesTheFullCatalogue()
    {
        var everyFeature = With([.. Enum.GetValues<TenantFeature>()]);

        Assert.Equal(
            sut.GetAllToolDefinitions().Select(t => t.Name),
            sut.GetToolDefinitions(everyFeature).Select(t => t.Name));
    }

    [Fact]
    public void GetToolDefinitions_ContainsCoreWriteTools()
    {
        var tools = sut.GetToolDefinitions(NoFeatures);
        var names = tools.Select(t => t.Name).ToHashSet();

        Assert.Contains("assign_load_to_truck", names);
        Assert.Contains("create_trip", names);
        Assert.Contains("dispatch_trip", names);
    }

    [Fact]
    public void GetToolDefinitions_HasUniqueToolNames()
    {
        var tools = sut.GetAllToolDefinitions();

        var names = tools.Select(t => t.Name).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }
}
