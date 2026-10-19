using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.TelegramBot.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Features;

namespace Logistics.TelegramBot.Services;

/// <summary>
/// On startup, records which tenant database each connected chat lives in, for every tenant with the
/// TelegramBot feature, so the bot can find a chat whose login state has since been purged.
/// </summary>
internal sealed class TelegramChatCacheWarmer(
    IServiceScopeFactory scopeFactory,
    ILogger<TelegramChatCacheWarmer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small delay to let other services start
        await Task.Delay(2000, stoppingToken);

        try
        {
            await WarmCacheAsync(stoppingToken);

            // Clean up expired login states on startup
            using var cleanupScope = scopeFactory.CreateScope();
            var authService = cleanupScope.ServiceProvider.GetRequiredService<TelegramAuthService>();
            await authService.CleanupStaleStatesAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to warm Telegram chat cache on startup");
        }
    }

    private async Task WarmCacheAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();

        var tenants = await masterUow.Repository<Tenant>().GetListAsync(ct: ct);
        var totalChats = 0;

        foreach (var tenant in tenants)
        {
            ct.ThrowIfCancellationRequested();

            if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.TelegramBot))
                continue;

            tenantUow.SetCurrentTenant(tenant);
            var chats = await tenantUow.Repository<TelegramChat>().GetListAsync(ct: ct);

            foreach (var chat in chats)
            {
                TelegramAuthService.RememberTenant(chat.ChatId, tenant.Id);
                totalChats++;
            }
        }

        logger.LogInformation(
            "Telegram chat cache warmed with {ChatCount} chats from {TenantCount} tenants",
            totalChats, tenants.Count);
    }
}
