using System.Collections.Concurrent;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.TelegramBot.Authentication;

internal sealed class TelegramAuthService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IFeatureService featureService,
    ILogger<TelegramAuthService> logger)
{
    private const string KeyPrefix = "logsx_";

    private static readonly ConcurrentDictionary<long, TelegramChatContext> Cache = new();

    public async Task<AuthResult> AuthenticateAsync(
        long chatId,
        string rawApiKey,
        TelegramChatType chatType,
        TelegramChatRole? role,
        string? username,
        string? firstName,
        string? groupTitle,
        CancellationToken ct = default)
    {
        if (!rawApiKey.StartsWith(KeyPrefix))
            return AuthResult.Fail("Invalid API key format.");

        if (!TryParseTenantId(rawApiKey, out var tenantId))
            return AuthResult.Fail("Invalid API key format.");

        var tenant = await masterUow.Repository<Tenant>()
            .GetAsync(t => t.Id == tenantId, ct);

        if (tenant is null)
            return AuthResult.Fail("Invalid API key.");

        tenantUow.SetCurrentTenant(tenant);

        var keyHash = ApiKeyHasher.Hash(rawApiKey);
        var apiKey = await tenantUow.Repository<ApiKey>()
            .GetAsync(k => k.KeyHash == keyHash, ct);

        if (apiKey is null)
            return AuthResult.Fail("Invalid API key.");

        if (!await featureService.IsFeatureEnabledAsync(tenantId, TenantFeature.TelegramBot))
            return AuthResult.Fail("Telegram Bot feature is not enabled for this tenant. Please upgrade your subscription plan.");

        // Upsert TelegramChat entity
        var existingChat = await tenantUow.Repository<TelegramChat>()
            .GetAsync(c => c.ChatId == chatId, ct);

        if (existingChat is not null)
        {
            existingChat.ApiKeyId = apiKey.Id;
            existingChat.ChatType = chatType;
            existingChat.Role = role;
            existingChat.Username = username;
            existingChat.FirstName = firstName;
            existingChat.GroupTitle = groupTitle;
            existingChat.ConnectedAt = DateTime.UtcNow;
        }
        else
        {
            existingChat = new TelegramChat
            {
                ChatId = chatId,
                ChatType = chatType,
                Role = role,
                Username = username,
                FirstName = firstName,
                GroupTitle = groupTitle,
                ApiKeyId = apiKey.Id
            };
            await tenantUow.Repository<TelegramChat>().AddAsync(existingChat, ct);
        }

        await tenantUow.SaveChangesAsync(ct);

        var context = new TelegramChatContext(tenantId, apiKey.Id, chatType, role, tenant.Name);
        Cache[chatId] = context;

        logger.LogInformation(
            "Telegram chat {ChatId} ({ChatType}) connected to tenant {TenantName}",
            chatId, chatType, tenant.Name);

        return AuthResult.Ok(context);
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

    public async Task UpdateRoleAsync(long chatId, TelegramChatRole role, CancellationToken ct = default)
    {
        var context = ResolveChatContext(chatId);
        if (context is null)
            return;

        await tenantUow.SetCurrentTenantByIdAsync(context.TenantId);
        var chat = await tenantUow.Repository<TelegramChat>()
            .GetAsync(c => c.ChatId == chatId, ct);

        if (chat is not null)
        {
            chat.Role = role;
            await tenantUow.SaveChangesAsync(ct);
        }

        Cache[chatId] = context with { Role = role };
    }

    public void WarmCache(long chatId, TelegramChatContext context)
    {
        Cache[chatId] = context;
    }

    public static void InvalidateCache(long chatId)
    {
        Cache.TryRemove(chatId, out _);
    }

    private static bool TryParseTenantId(string rawKey, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        var afterPrefix = rawKey[KeyPrefix.Length..];
        var underscoreIndex = afterPrefix.IndexOf('_');
        if (underscoreIndex <= 0)
            return false;

        return Guid.TryParse(afterPrefix[..underscoreIndex], out tenantId);
    }
}

internal sealed record AuthResult(bool IsSuccess, string? Error, TelegramChatContext? Context)
{
    public static AuthResult Ok(TelegramChatContext context) => new(true, null, context);
    public static AuthResult Fail(string error) => new(false, error, null);
}
