using System.Reflection;
using System.Runtime.CompilerServices;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Infrastructure.AI;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

/// <summary>
/// A tool only works when it appears in three places at once: the class, the DI roster in
/// <see cref="Registrar"/>, and <c>AIDispatchToolRegistry.Tools</c>. Nothing made those agree, and
/// both directions fail silently - a definition with no implementation reaches the model and comes
/// back "Unknown tool", while an implementation with no definition is never mentioned to the model
/// at all, so no surface can ever reach it.
/// </summary>
public class AIDispatchToolRegistryParityTests
{
    private static readonly AIDispatchToolRegistry Registry = new();

    private static List<Type> DiRegisteredToolTypes()
    {
        var services = new ServiceCollection();
        services.AddAIInfrastructure(new ConfigurationBuilder().Build());

        return [.. services
            .Where(d => d.ServiceType == typeof(IAgentTool))
            .Select(d => d.ImplementationType)
            .OfType<Type>()];
    }

    private static List<Type> DeclaredToolTypes() =>
        [.. typeof(Registrar).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IAgentTool).IsAssignableFrom(t))];

    /// <summary>
    /// Reads <see cref="IAgentTool.Name"/> without running a constructor. Every tool implements
    /// it as an expression-bodied literal that touches no injected state, so an uninitialized
    /// instance answers correctly and we avoid substituting 29 different dependency sets.
    /// </summary>
    private static string NameOf(Type toolType) =>
        ((IAgentTool)RuntimeHelpers.GetUninitializedObject(toolType)).Name;

    [Fact]
    public void EveryDeclaredTool_IsRegisteredInDi()
    {
        var missing = DeclaredToolTypes()
            .Except(DiRegisteredToolTypes())
            .Select(t => t.Name)
            .Order()
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"Tools implementing IAgentTool but never registered in Registrar: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryRegistryDefinition_HasAnImplementation()
    {
        var implemented = DiRegisteredToolTypes().Select(NameOf).ToHashSet();

        var orphanDefinitions = Registry.GetAllToolDefinitions()
            .Select(d => d.Name)
            .Where(name => !implemented.Contains(name))
            .Order()
            .ToList();

        Assert.True(
            orphanDefinitions.Count == 0,
            "Tool definitions with no registered implementation - the model is told these exist and "
            + $"gets 'Unknown tool' back: {string.Join(", ", orphanDefinitions)}");
    }

    [Fact]
    public void EveryImplementation_HasARegistryDefinition()
    {
        var defined = Registry.GetAllToolDefinitions().Select(d => d.Name).ToHashSet();

        var unreachable = DiRegisteredToolTypes()
            .Where(t => !defined.Contains(NameOf(t)))
            .Select(t => $"{t.Name} ('{NameOf(t)}')")
            .Order()
            .ToList();

        Assert.True(
            unreachable.Count == 0,
            "Tools registered in DI but absent from AIDispatchToolRegistry.Tools - unreachable from "
            + $"the dispatch agent, the copilot, and MCP alike: {string.Join(", ", unreachable)}");
    }

    [Fact]
    public void ToolNames_AreUniqueAcrossImplementations()
    {
        var duplicates = DiRegisteredToolTypes()
            .GroupBy(NameOf)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Order()
            .ToList();

        Assert.True(duplicates.Count == 0, $"Duplicate tool names: {string.Join(", ", duplicates)}");
    }

    [Fact]
    public void ToolNames_AreSnakeCase()
    {
        Assert.All(DiRegisteredToolTypes(), t =>
        {
            var name = NameOf(t);
            Assert.Matches("^[a-z][a-z0-9_]*$", name);
        });
    }
}
