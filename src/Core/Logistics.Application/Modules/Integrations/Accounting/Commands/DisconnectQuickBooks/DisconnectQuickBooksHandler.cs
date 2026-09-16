using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

internal sealed class DisconnectQuickBooksHandler(
    ITenantUnitOfWork tenantUow,
    IAccountingProviderFactory providerFactory,
    ILogger<DisconnectQuickBooksHandler> logger)
    : IRequestHandler<DisconnectQuickBooksCommand, Result>
{
    public async Task<Result> Handle(DisconnectQuickBooksCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<AccountingProviderConfiguration>()
            .GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline, ct);

        if (config is null || !config.IsActive)
        {
            return Result.Ok();
        }

        if (!string.IsNullOrEmpty(config.RefreshToken))
        {
            try
            {
                var provider = providerFactory.GetProvider(config);
                await provider.RevokeAsync(config.RefreshToken, ct);
            }
            catch (Exception ex)
            {
                // Revoke is best-effort; still deactivate locally so the tenant is disconnected.
                logger.LogWarning(ex, "QuickBooks token revoke failed during disconnect; deactivating locally");
            }
        }

        config.IsActive = false;
        config.AccessToken = null;
        config.RefreshToken = null;
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Disconnected QuickBooks Online for tenant {TenantId}",
            tenantUow.GetCurrentTenant().Id);
        return Result.Ok();
    }
}
