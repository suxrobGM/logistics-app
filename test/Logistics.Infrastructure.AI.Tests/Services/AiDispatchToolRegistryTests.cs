using Logistics.Infrastructure.AI.Services;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AiDispatchToolRegistryTests
{
    private readonly AiDispatchToolRegistry sut = new();

    [Fact]
    public void GetToolDefinitions_ReturnsNonEmptyList()
    {
        var tools = sut.GetToolDefinitions();

        Assert.NotEmpty(tools);
    }

    [Fact]
    public void GetToolDefinitions_AllToolsHaveNameAndDescription()
    {
        var tools = sut.GetToolDefinitions();

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
        var tools = sut.GetToolDefinitions();

        Assert.DoesNotContain(tools, t => t.Name == "search_loadboard");
        Assert.DoesNotContain(tools, t => t.Name == "book_loadboard_load");
    }

    [Fact]
    public void GetToolDefinitions_IncludesLoadBoardTools_WhenRequested()
    {
        var tools = sut.GetToolDefinitions(includeLoadBoardTools: true, includeIntermodalTools: true);

        Assert.Contains(tools, t => t.Name == "search_loadboard");
        Assert.Contains(tools, t => t.Name == "book_loadboard_load");
    }

    [Fact]
    public void GetToolDefinitions_ContainsCoreReadTools()
    {
        var tools = sut.GetToolDefinitions();
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
        var names = sut.GetToolDefinitions(includeIntermodalTools: true).Select(t => t.Name).ToHashSet();

        Assert.Contains("get_container_status", names);
        Assert.Contains("get_terminal_info", names);
    }

    /// <summary>
    /// Their schemas cost tokens on every request, so a tenant without the feature must not get them.
    /// </summary>
    [Fact]
    public void GetToolDefinitions_WithoutIntermodalFeature_OmitsTheIntermodalTools()
    {
        var names = sut.GetToolDefinitions().Select(t => t.Name).ToHashSet();

        Assert.DoesNotContain("get_container_status", names);
        Assert.DoesNotContain("get_terminal_info", names);
    }

    /// <summary>The two gated groups are independent - neither switch may pull in the other.</summary>
    [Fact]
    public void GetToolDefinitions_GatedGroupsAreIndependent()
    {
        var loadBoardOnly = sut.GetToolDefinitions(includeLoadBoardTools: true)
            .Select(t => t.Name).ToHashSet();
        Assert.Contains("search_loadboard", loadBoardOnly);
        Assert.DoesNotContain("get_container_status", loadBoardOnly);

        var intermodalOnly = sut.GetToolDefinitions(includeIntermodalTools: true)
            .Select(t => t.Name).ToHashSet();
        Assert.Contains("get_container_status", intermodalOnly);
        Assert.DoesNotContain("search_loadboard", intermodalOnly);
    }

    [Fact]
    public void GetToolDefinitions_ContainsCoreWriteTools()
    {
        var tools = sut.GetToolDefinitions();
        var names = tools.Select(t => t.Name).ToHashSet();

        Assert.Contains("assign_load_to_truck", names);
        Assert.Contains("create_trip", names);
        Assert.Contains("dispatch_trip", names);
    }

    [Fact]
    public void GetToolDefinitions_HasUniqueToolNames()
    {
        var tools = sut.GetToolDefinitions(includeLoadBoardTools: true, includeIntermodalTools: true);

        var names = tools.Select(t => t.Name).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }
}
