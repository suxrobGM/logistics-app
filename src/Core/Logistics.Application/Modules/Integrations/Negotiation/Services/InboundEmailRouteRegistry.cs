using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// Owns the master-database reply-address routes. A route that outlives its thread is an open door
/// into a tenant's inbox, so every path that opens or closes a thread goes through here rather than
/// each remembering to stamp the row itself.
/// </summary>
public interface IInboundEmailRouteRegistry : IApplicationService
{
    Task OpenAsync(string threadToken, Guid tenantId, DateTime? expiresAt, CancellationToken ct = default);

    /// <summary>
    /// Restamps the reply window on an existing route, creating it when the row is missing - a
    /// thread whose route vanished would otherwise silently drop every reply for its whole life.
    /// </summary>
    Task RefreshAsync(string threadToken, Guid tenantId, DateTime? expiresAt, CancellationToken ct = default);

    Task RevokeAsync(IEnumerable<string> threadTokens, CancellationToken ct = default);
}

internal sealed class InboundEmailRouteRegistry(IMasterUnitOfWork masterUow) : IInboundEmailRouteRegistry
{
    public async Task OpenAsync(
        string threadToken, Guid tenantId, DateTime? expiresAt, CancellationToken ct = default)
    {
        await masterUow.Repository<InboundEmailRoute>().AddAsync(new InboundEmailRoute
        {
            ThreadToken = threadToken,
            TenantId = tenantId,
            Purpose = InboundEmailPurpose.RateNegotiation,
            ExpiresAt = expiresAt
        }, ct);

        await masterUow.SaveChangesAsync(ct);
    }

    public async Task RefreshAsync(
        string threadToken, Guid tenantId, DateTime? expiresAt, CancellationToken ct = default)
    {
        var route = await masterUow.Repository<InboundEmailRoute>()
            .GetAsync(r => r.ThreadToken == threadToken, ct);

        if (route is null)
        {
            await OpenAsync(threadToken, tenantId, expiresAt, ct);
            return;
        }

        route.ExpiresAt = expiresAt;
        await masterUow.SaveChangesAsync(ct);
    }

    public async Task RevokeAsync(IEnumerable<string> threadTokens, CancellationToken ct = default)
    {
        var tokens = threadTokens.ToArray();
        if (tokens.Length == 0)
        {
            return;
        }

        var routes = await masterUow.Repository<InboundEmailRoute>()
            .GetListAsync(r => tokens.Contains(r.ThreadToken) && r.RevokedAt == null, ct);

        if (routes.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var route in routes)
        {
            route.RevokedAt = now;
        }

        await masterUow.SaveChangesAsync(ct);
    }
}
