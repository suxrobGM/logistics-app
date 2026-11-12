using System.Security.Claims;
using Logistics.Application.Modules.Platform.Notifications.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.IdentityServer.Services;

/// <summary>
/// Finishes the Telegram bot's login link: links the chat to the signed-in employee in the tenant
/// database, then marks the login state consumed in the master database so the bot can pick it up.
/// </summary>
public class TelegramLoginService(
    UserManager<User> userManager,
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow)
{
    public async Task<Result<TelegramChatConnection>> ConnectChatAsync(
        string? state,
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(state))
        {
            return Result<TelegramChatConnection>.Fail("Invalid login link. Please try again from the Telegram bot.");
        }

        var loginState = await masterUow.Repository<TelegramLoginState>()
            .GetAsync(s => s.State == state && !s.IsConsumed, ct);

        if (loginState is null)
        {
            return Result<TelegramChatConnection>.Fail("Login link has already been used or is invalid.");
        }

        if (loginState.ExpiresAt < DateTime.UtcNow)
        {
            return Result<TelegramChatConnection>.Fail("Login link has expired. Please request a new one from the bot.");
        }

        var user = await userManager.GetUserAsync(principal);
        if (user?.TenantId is null)
        {
            return Result<TelegramChatConnection>.Fail(
                "Unable to resolve your account. Please ensure you belong to a company.");
        }

        var tenant = await masterUow.Repository<Tenant>()
            .GetAsync(t => t.Id == user.TenantId.Value, ct);

        if (tenant is null)
        {
            return Result<TelegramChatConnection>.Fail("Unable to resolve your company.");
        }

        tenantUow.SetCurrentTenant(tenant);
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(user.Id, ct);

        if (employee is null)
        {
            return Result<TelegramChatConnection>.Fail("Only employees of your company can connect the bot.");
        }

        var chatRole = TelegramRecipients.GetChatRole(employee.Role?.Name);
        var isGroupChat = loginState.ChatType == TelegramChatType.Group;
        if (isGroupChat && chatRole != TelegramChatRole.Dispatcher)
        {
            return Result<TelegramChatConnection>.Fail(
                "Only owners, managers, or dispatchers can connect group chats.");
        }

        await SaveChatAsync(loginState, user.Id, ct);

        loginState.IsConsumed = true;
        loginState.TenantId = tenant.Id;
        await masterUow.SaveChangesAsync(ct);

        return Result<TelegramChatConnection>.Ok(
            new TelegramChatConnection(tenant.Name, user.GetFullName(), chatRole?.ToString()));
    }

    private async Task SaveChatAsync(TelegramLoginState loginState, Guid userId, CancellationToken ct)
    {
        var existingChat = await tenantUow.Repository<TelegramChat>()
            .GetAsync(c => c.ChatId == loginState.ChatId, ct);

        if (existingChat is not null)
        {
            existingChat.UserId = userId;
            existingChat.ChatType = loginState.ChatType;
            existingChat.ConnectedAt = DateTime.UtcNow;
        }
        else
        {
            var newChat = new TelegramChat
            {
                ChatId = loginState.ChatId,
                ChatType = loginState.ChatType,
                UserId = userId
            };
            await tenantUow.Repository<TelegramChat>().AddAsync(newChat, ct);
        }

        await tenantUow.SaveChangesAsync(ct);
    }
}
