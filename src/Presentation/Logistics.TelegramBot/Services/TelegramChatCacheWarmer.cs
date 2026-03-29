using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.TelegramBot.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Logistics.TelegramBot.Services;

/// <summary>
/// Background service that warms the Telegram chat cache on application startup.
/// This ensures that notifications can be sent immediately without waiting for the first cache miss.
/// The cache is warmed by loading all tenants with the TelegramBot feature enabled,
/// then loading their Telegram chats and populating the authentication cache with the chat contexts.
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
                var context = new TelegramChatContext(
                    tenant.Id,
                    chat.UserId,
                    chat.ChatType,
                    chat.Role,
                    tenant.Name);

                TelegramAuthService.WarmCache(chat.ChatId, context);
                totalChats++;
            }
        }

        logger.LogInformation(
            "Telegram chat cache warmed with {ChatCount} chats from {TenantCount} tenants",
            totalChats, tenants.Count);
    }
}
