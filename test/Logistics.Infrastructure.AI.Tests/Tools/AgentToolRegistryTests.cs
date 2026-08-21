using Logistics.Shared.Identity.Policies;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class AgentToolRegistryTests
{
    private readonly AgentToolRegistry sut = new();

    /// <summary>A tenant with no gated features - the baseline every ungated tool must survive.</summary>
    private static readonly IReadOnlySet<TenantFeature> NoFeatures = new HashSet<TenantFeature>();

    /// <summary>Every permission in the system - isolates the feature filter from the permission one.</summary>
    private static readonly IReadOnlySet<string> EveryPermission = Permission.GetAll().ToHashSet();

    private static IReadOnlySet<TenantFeature> With(params TenantFeature[] features) =>
        new HashSet<TenantFeature>(features);

    [Fact]
    public void GetCopilotTools_ReturnsNonEmptyList()
    {
        var tools = sut.GetCopilotTools(NoFeatures, EveryPermission);

        Assert.NotEmpty(tools);
    }

    [Fact]
    public void GetCopilotTools_AllToolsHaveNameAndDescription()
    {
        var tools = sut.GetCopilotTools(NoFeatures, EveryPermission);

        foreach (var tool in tools)
        {
            Assert.False(string.IsNullOrWhiteSpace(tool.Name), "Tool has empty name");
            Assert.False(string.IsNullOrWhiteSpace(tool.Description), $"Tool '{tool.Name}' has empty description");
            Assert.NotNull(tool.InputSchema);
        }
    }

    [Fact]
    public void GetCopilotTools_ExcludesLoadBoardTools_ByDefault()
    {
        var tools = sut.GetCopilotTools(NoFeatures, EveryPermission);

        Assert.DoesNotContain(tools, t => t.Name == "search_loadboard");
        Assert.DoesNotContain(tools, t => t.Name == "book_loadboard_load");
    }

    [Fact]
    public void GetCopilotTools_IncludesLoadBoardTools_WhenRequested()
    {
        var tools = sut.GetAllTools();

        Assert.Contains(tools, t => t.Name == "search_loadboard");
        Assert.Contains(tools, t => t.Name == "book_loadboard_load");
    }

    [Fact]
    public void GetCopilotTools_ContainsCoreReadTools()
    {
        var tools = sut.GetCopilotTools(NoFeatures, EveryPermission);
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
    public void GetCopilotTools_IntermodalToolsAreReadsNotWrites()
    {
        var names = sut.GetCopilotTools(With(TenantFeature.IntermodalContainers), EveryPermission).Select(t => t.Name).ToHashSet();

        Assert.Contains("get_container_status", names);
        Assert.Contains("get_terminal_info", names);
    }

    /// <summary>
    /// Their schemas cost tokens on every request, so a tenant without the feature must not get them.
    /// </summary>
    [Fact]
    public void GetCopilotTools_WithoutIntermodalFeature_OmitsTheIntermodalTools()
    {
        var names = sut.GetCopilotTools(NoFeatures, EveryPermission).Select(t => t.Name).ToHashSet();

        Assert.DoesNotContain("get_container_status", names);
        Assert.DoesNotContain("get_terminal_info", names);
    }

    /// <summary>The gated groups are independent - one feature may not pull in another's tools.</summary>
    [Fact]
    public void GetCopilotTools_GatedGroupsAreIndependent()
    {
        var loadBoardOnly = sut.GetCopilotTools(With(TenantFeature.LoadBoard), EveryPermission)
            .Select(t => t.Name).ToHashSet();
        Assert.Contains("search_loadboard", loadBoardOnly);
        Assert.DoesNotContain("get_container_status", loadBoardOnly);

        var intermodalOnly = sut.GetCopilotTools(With(TenantFeature.IntermodalContainers), EveryPermission)
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
    public void GetAllTools_CarriesTheFeatureEachGatedToolNeeds()
    {
        var all = sut.GetAllTools().ToDictionary(t => t.Name, t => t.RequiredFeature);

        Assert.Equal(TenantFeature.LoadBoard, all["search_loadboard"]);
        Assert.Equal(TenantFeature.LoadBoard, all["check_broker_credit"]);
        Assert.Equal(TenantFeature.LoadBoard, all["book_loadboard_load"]);
        Assert.Equal(TenantFeature.IntermodalContainers, all["get_container_status"]);
        Assert.Equal(TenantFeature.IntermodalContainers, all["get_terminal_info"]);
        Assert.Null(all["get_unassigned_loads"]);
    }

    /// <summary>Every gated tool must be reachable - a feature nobody grants is a dead tool.</summary>
    [Fact]
    public void GetCopilotTools_WithEveryFeature_MatchesTheFullCatalogue()
    {
        var everyFeature = With([.. Enum.GetValues<TenantFeature>()]);

        Assert.Equal(
            sut.GetAllTools().Select(t => t.Name),
            sut.GetCopilotTools(everyFeature, EveryPermission).Select(t => t.Name));
    }

    [Fact]
    public void GetCopilotTools_ContainsCoreWriteTools()
    {
        var tools = sut.GetCopilotTools(NoFeatures, EveryPermission);
        var names = tools.Select(t => t.Name).ToHashSet();

        Assert.Contains("assign_load_to_truck", names);
        Assert.Contains("create_trip", names);
        Assert.Contains("dispatch_trip", names);
    }

    [Fact]
    public void GetCopilotTools_HasUniqueToolNames()
    {
        var tools = sut.GetAllTools();

        var names = tools.Select(t => t.Name).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    #region Permission scoping

    private static readonly IReadOnlySet<TenantFeature> EveryFeature =
        With([.. Enum.GetValues<TenantFeature>()]);

    /// <summary>An exposed copilot write tool would run against the whole fleet, not one caller's turn.</summary>
    [Fact]
    public void GetDispatchAgentTools_ExcludesCopilotTools()
    {
        var names = sut.GetDispatchAgentTools(EveryFeature)
            .Select(t => t.Name).ToHashSet();

        Assert.Contains("get_unassigned_loads", names);
        Assert.Contains("assign_load_to_truck", names);
        Assert.DoesNotContain("create_load_invoice", names);
        Assert.DoesNotContain("send_invoice", names);
        Assert.DoesNotContain("search_loads", names);
    }

    /// <summary>
    /// The dispatch catalogue must not be a by-product of which permission a tool happens to name -
    /// picking a non-Dispatch permission for a new dispatch tool would silently drop it.
    /// </summary>
    [Fact]
    public void GetDispatchAgentTools_IsIndependentOfTheToolsPermission()
    {
        var dispatchTools = sut.GetDispatchAgentTools(EveryFeature);

        Assert.All(dispatchTools, t => Assert.True(t.DispatchAgent));
        Assert.Equal(
            sut.GetAllTools().Where(t => t.DispatchAgent).Select(t => t.Name),
            dispatchTools.Select(t => t.Name));
    }

    /// <summary>
    /// A write tool the dispatch agent may call runs unattended against the whole fleet the moment
    /// it opts in, so doing so is a deliberate act - this pins the current set.
    /// </summary>
    [Fact]
    public void GetAllTools_DispatchAgentWriteToolsAreAKnownSet()
    {
        var writeTools = sut.GetAllTools()
            .Where(t => t is { DispatchAgent: true, IsWrite: true })
            .Select(t => t.Name)
            .OrderBy(n => n);

        Assert.Equal(
            ["assign_load_to_truck", "book_loadboard_load", "create_trip", "dispatch_trip",
             "propose_counter_offer"],
            writeTools);
    }

    [Fact]
    public void GetCopilotTools_CallerWithInvoicePermissions_SeesInvoiceToolsOnly()
    {
        var invoiceScope = new HashSet<string> { "Permission.Invoice.View", "Permission.Invoice.Manage" };

        var names = sut.GetCopilotTools(EveryFeature, invoiceScope).Select(t => t.Name).ToHashSet();

        Assert.Contains("get_invoices", names);
        Assert.Contains("create_load_invoice", names);
        Assert.Contains("send_invoice", names);
        Assert.DoesNotContain("assign_load_to_truck", names);
        Assert.DoesNotContain("create_payment_link", names);
    }

    /// <summary>Every tool must declare a permission - an undeclared one bypasses copilot scoping.</summary>
    [Fact]
    public void GetAllTools_EveryToolDeclaresARequiredPermission()
    {
        Assert.All(sut.GetAllTools(), t =>
            Assert.False(string.IsNullOrWhiteSpace(t.RequiredPermission), $"Tool '{t.Name}' has no RequiredPermission"));
    }

    [Fact]
    public void TryGetDefinition_KnownAndUnknownNames()
    {
        Assert.NotNull(sut.TryGetDefinition("create_load_invoice"));
        Assert.Null(sut.TryGetDefinition("hallucinated_tool"));
    }

    #endregion
}
