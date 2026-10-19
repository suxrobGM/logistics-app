using System.Collections.Concurrent;
using System.Security.Cryptography;
using Logistics.Application.Modules.Platform.Notifications.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.TelegramBot.Authentication;

internal sealed class TelegramAuthService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    ILogger<TelegramAuthService> logger)
{
    /// <summary>
    /// Which tenant database each connected chat lives in. A routing hint only: every update is
    /// still authorized against that database.
    /// </summary>
    private static readonly ConcurrentDictionary<long, Guid> TenantByChat = new();

    /// <summary>
    /// Creates a login state and returns the state token for the login URL.
    /// </summary>
    public async Task<string> CreateLoginStateAsync(
        long chatId,
        TelegramChatType chatType,
        CancellationToken ct = default)
    {
        // Remove any existing pending states for this chat
        var staleStates = await masterUow.Repository<TelegramLoginState>()
            .GetListAsync(s => s.ChatId == chatId && !s.IsConsumed, ct: ct);

        foreach (var stale in staleStates)
        {
            masterUow.Repository<TelegramLoginState>().Delete(stale);
        }

        var state = GenerateState();

        var loginState = new TelegramLoginState
        {
            State = state,
            ChatId = chatId,
            ChatType = chatType,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await masterUow.Repository<TelegramLoginState>().AddAsync(loginState, ct);
        await masterUow.SaveChangesAsync(ct);
        return state;
    }

    /// <summary>
    /// Removes expired and consumed login states older than the specified age.
    /// </summary>
    public async Task CleanupStaleStatesAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-1);
        var staleStates = await masterUow.Repository<TelegramLoginState>()
            .GetListAsync(s => s.ExpiresAt < cutoff, ct: ct);

        if (staleStates.Count == 0)
            return;

        foreach (var state in staleStates)
        {
            masterUow.Repository<TelegramLoginState>().Delete(state);
        }

        await masterUow.SaveChangesAsync(ct);
        logger.LogInformation("Cleaned up {Count} stale Telegram login states", staleStates.Count);
    }

    /// <summary>
    /// Completes a login from its state token. Returns the context once the login is consumed and
    /// the chat authorizes, null while it is still pending.
    /// </summary>
    public async Task<TelegramChatContext?> CheckLoginStateAsync(
        string state,
        CancellationToken ct = default)
    {
        var loginState = await masterUow.Repository<TelegramLoginState>()
            .GetAsync(s => s.State == state, ct);

        if (loginState is null || !loginState.IsConsumed || loginState.TenantId is null)
            return null;

        TenantByChat[loginState.ChatId] = loginState.TenantId.Value;
        return await ResolveChatContextAsync(loginState.ChatId, ct);
    }

    /// <summary>
    /// Authorizes the chat against its tenant database on every call: the chat row must still exist
    /// and its connecting employee must still be Active. Also switches <see cref="ITenantUnitOfWork"/>
    /// to that tenant.
    /// </summary>
    public async Task<TelegramChatContext?> ResolveChatContextAsync(long chatId, CancellationToken ct = default)
    {
        var tenant = await FindTenantAsync(chatId, ct);
        if (tenant is null)
        {
            TenantByChat.TryRemove(chatId, out _);
            return null;
        }

        tenantUow.SetCurrentTenant(tenant);
        var chat = await TelegramRecipients.FindAuthorizedChatAsync(tenantUow, chatId, ct);

        if (chat is null)
        {
            TenantByChat.TryRemove(chatId, out _);
            return null;
        }

        TenantByChat[chatId] = tenant.Id;
        return new TelegramChatContext(tenant.Name);
    }

    public async Task<bool> DisconnectAsync(long chatId, CancellationToken ct = default)
    {
        var tenant = await FindTenantAsync(chatId, ct);
        TenantByChat.TryRemove(chatId, out _);

        if (tenant is null)
            return false;

        tenantUow.SetCurrentTenant(tenant);
        var chat = await tenantUow.Repository<TelegramChat>()
            .GetAsync(c => c.ChatId == chatId, ct);

        if (chat is null)
            return false;

        tenantUow.Repository<TelegramChat>().Delete(chat);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Telegram chat {ChatId} disconnected from tenant {TenantName}",
            chatId, tenant.Name);

        return true;
    }

    public static void RememberTenant(long chatId, Guid tenantId)
    {
        TenantByChat[chatId] = tenantId;
    }

    /// <summary>
    /// The tenant whose database holds the chat's row. Login states are purged an hour after they
    /// expire, so a chat connected before the last restart is found only through the map the
    /// cache warmer fills from the tenant databases; a chat connected since, only through its
    /// latest consumed login state.
    /// </summary>
    private async Task<Tenant?> FindTenantAsync(long chatId, CancellationToken ct)
    {
        Guid? tenantId = TenantByChat.TryGetValue(chatId, out var known)
            ? known
            : await masterUow.Repository<TelegramLoginState>().Query()
                .Where(s => s.ChatId == chatId && s.IsConsumed && s.TenantId != null)
                .OrderByDescending(s => s.ExpiresAt)
                .Select(s => s.TenantId)
                .FirstOrDefaultAsync(ct);

        return tenantId is null
            ? null
            : await masterUow.Repository<Tenant>().GetByIdAsync(tenantId.Value, ct);
    }

    private static string GenerateState()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
