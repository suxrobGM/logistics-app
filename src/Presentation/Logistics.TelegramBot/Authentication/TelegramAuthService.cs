using System.Collections.Concurrent;
using System.Security.Cryptography;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.TelegramBot.Authentication;

internal sealed class TelegramAuthService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    ILogger<TelegramAuthService> logger)
{
    private static readonly ConcurrentDictionary<long, TelegramChatContext> Cache = new();

    /// <summary>
    /// Creates a login state and returns the state token for the login URL.
    /// </summary>
    public async Task<string> CreateLoginStateAsync(
        long chatId,
        TelegramChatType chatType,
        CancellationToken ct = default)
    {
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
    /// Polls for a consumed login state. Returns the context if login is complete, null if still pending.
    /// </summary>
    public async Task<TelegramChatContext?> CheckLoginStateAsync(
        string state,
        CancellationToken ct = default)
    {
        var loginState = await masterUow.Repository<TelegramLoginState>()
            .GetAsync(s => s.State == state, ct);

        if (loginState is null || !loginState.IsConsumed)
            return null;

        if (loginState.TenantId is null || loginState.TenantName is null)
            return null;

        var context = new TelegramChatContext(
            loginState.TenantId.Value,
            loginState.UserId,
            loginState.ChatType,
            loginState.ResolvedRole,
            loginState.TenantName);

        // Cache the context
        Cache[loginState.ChatId] = context;

        logger.LogInformation(
            "Telegram chat {ChatId} authenticated via Identity Server for tenant {TenantName}",
            loginState.ChatId, loginState.TenantName);

        return context;
    }

    public TelegramChatContext? ResolveChatContext(long chatId)
    {
        return Cache.TryGetValue(chatId, out var context) ? context : null;
    }

    public async Task<bool> DisconnectAsync(long chatId, CancellationToken ct = default)
    {
        var context = ResolveChatContext(chatId);
        if (context is null)
            return false;

        await tenantUow.SetCurrentTenantByIdAsync(context.TenantId);
        var chat = await tenantUow.Repository<TelegramChat>()
            .GetAsync(c => c.ChatId == chatId, ct);

        if (chat is not null)
        {
            tenantUow.Repository<TelegramChat>().Delete(chat);
            await tenantUow.SaveChangesAsync(ct);
        }

        Cache.TryRemove(chatId, out _);

        logger.LogInformation(
            "Telegram chat {ChatId} disconnected from tenant {TenantName}",
            chatId, context.TenantName);

        return true;
    }

    public static void WarmCache(long chatId, TelegramChatContext context)
    {
        Cache[chatId] = context;
    }

    private static string GenerateState()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
