using System.Reflection;
using System.Runtime.CompilerServices;
using Logistics.Infrastructure.AI;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

/// <summary>
/// A tool has to appear in two places at once: the class, and <c>AgentToolRegistry.Tools</c>.
/// Nothing makes those agree, and both directions fail silently - a definition with no
/// implementation reaches the model and comes back "Unknown tool", while an implementation with no
/// definition is never mentioned to the model at all, so no surface can ever reach it.
///
/// DI used to be a third place, guarded by an EveryDeclaredTool_IsRegisteredInDi test. Registrar
/// now discovers tools by scanning the assembly, so that test compared a reflection scan against
/// itself and could never fail; it was deleted rather than left as a no-op. A broken scan predicate
/// still fails loudly here - zero registrations makes every definition an orphan.
/// </summary>
public class AgentToolRegistryParityTests
{
    private static readonly AgentToolRegistry Registry = new();

    private static List<Type> DiRegisteredToolTypes()
    {
        var services = new ServiceCollection();
        services.AddAIInfrastructure(new ConfigurationBuilder().Build());

        return [.. services
            .Where(d => d.ServiceType == typeof(IAgentTool))
            .Select(d => d.ImplementationType)
            .OfType<Type>()];
    }

    /// <summary>
    /// Reads <see cref="IAgentTool.Name"/> without running a constructor. Every tool implements
    /// it as an expression-bodied literal that touches no injected state, so an uninitialized
    /// instance answers correctly and we avoid substituting 29 different dependency sets.
    /// </summary>
    private static string NameOf(Type toolType) =>
        ((IAgentTool)RuntimeHelpers.GetUninitializedObject(toolType)).Name;

    [Fact]
    public void EveryRegistryDefinition_HasAnImplementation()
    {
        var implemented = DiRegisteredToolTypes().Select(NameOf).ToHashSet();

        var orphanDefinitions = Registry.GetAllTools()
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
        var defined = Registry.GetAllTools().Select(d => d.Name).ToHashSet();

        var unreachable = DiRegisteredToolTypes()
            .Where(t => !defined.Contains(NameOf(t)))
            .Select(t => $"{t.Name} ('{NameOf(t)}')")
            .Order()
            .ToList();

        Assert.True(
            unreachable.Count == 0,
            "Tools registered in DI but absent from AgentToolRegistry.Tools - unreachable from "
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
