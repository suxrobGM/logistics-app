using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Loads;

/// <summary>
/// Resolves a page of loads' container and terminal values in two queries. Without it
/// <c>LoadMapper</c> reads them off navigation properties, turning a 25-row page into up to 75 extra
/// SELECTs. Use it in any handler that maps more than one load.
/// </summary>
internal static class LoadIntermodalResolver
{
    public static async Task<LoadIntermodalLookup> ResolveAsync(
        ITenantUnitOfWork uow,
        IReadOnlyCollection<Load> loads,
        CancellationToken ct = default)
    {
        var containerIds = loads
            .Select(l => l.ContainerId)
            .OfType<Guid>()
            .Distinct()
            .ToArray();

        var terminalIds = loads
            .SelectMany(l => new[] { l.OriginTerminalId, l.DestinationTerminalId })
            .OfType<Guid>()
            .Distinct()
            .ToArray();

        if (containerIds.Length == 0 && terminalIds.Length == 0)
        {
            return LoadIntermodalLookup.Empty;
        }

        var containers = containerIds.Length == 0
            ? []
            : await uow.Repository<Container>().Query()
                .Where(c => containerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => new ContainerRef(c.Number, c.IsoType), ct);

        var terminals = terminalIds.Length == 0
            ? []
            : await uow.Repository<Terminal>().Query()
                .Where(t => terminalIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => new TerminalRef(t.Name, t.Code), ct);

        return new LoadIntermodalLookup(containers, terminals);
    }
}
