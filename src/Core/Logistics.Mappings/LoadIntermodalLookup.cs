using Logistics.Domain.Primitives.Enums;

namespace Logistics.Mappings;

/// <summary>Container display values for one load.</summary>
public readonly record struct ContainerRef(string Number, ContainerIsoType IsoType);

/// <summary>Terminal display values for one load.</summary>
public readonly record struct TerminalRef(string Name, string Code);

/// <summary>
/// Pre-resolved container and terminal values for a batch of loads. Reading them off the navigation
/// properties inside the mapper costs up to three lazy SELECTs per row - the N+1 that
/// <c>.claude/rules/backend/mapperly.md</c> forbids. List handlers resolve them once and pass this in.
/// </summary>
public sealed class LoadIntermodalLookup(
    IReadOnlyDictionary<Guid, ContainerRef> containers,
    IReadOnlyDictionary<Guid, TerminalRef> terminals)
{
    /// <summary>
    /// For single-row handlers, where one lazy load is not an N+1 - <c>ToDto</c> then falls back to
    /// the navigation properties.
    /// </summary>
    public static LoadIntermodalLookup Empty { get; } =
        new(new Dictionary<Guid, ContainerRef>(), new Dictionary<Guid, TerminalRef>());

    public ContainerRef? FindContainer(Guid? id) =>
        id is { } key && containers.TryGetValue(key, out var found) ? found : null;

    public TerminalRef? FindTerminal(Guid? id) =>
        id is { } key && terminals.TryGetValue(key, out var found) ? found : null;
}
