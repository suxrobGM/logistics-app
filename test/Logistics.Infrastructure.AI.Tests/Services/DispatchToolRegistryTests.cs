using Logistics.Infrastructure.AI.Services;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class DispatchToolRegistryTests
{
    private readonly DispatchToolRegistry sut = new();

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
        var tools = sut.GetToolDefinitions(true);

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
        var tools = sut.GetToolDefinitions(true);

        var names = tools.Select(t => t.Name).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }
}
