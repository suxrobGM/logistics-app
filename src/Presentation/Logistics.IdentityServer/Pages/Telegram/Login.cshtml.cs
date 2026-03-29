using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Telegram;

[SecurityHeaders]
[Authorize]
public class Login(
    UserManager<User> userManager,
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow) : PageModel
{
    public string? ErrorMessage { get; set; }
    public string? TenantName { get; set; }
    public string? UserName { get; set; }
    public string? RoleDisplay { get; set; }
    public bool Success { get; set; }

    public async Task<IActionResult> OnGet([FromQuery] string? state, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(state))
        {
            ErrorMessage = "Invalid login link. Please try again from the Telegram bot.";
            return Page();
        }

        // Find the login state
        var loginState = await masterUow.Repository<TelegramLoginState>()
            .GetAsync(s => s.State == state && !s.IsConsumed, ct);

        if (loginState is null)
        {
            ErrorMessage = "Login link has already been used or is invalid.";
            return Page();
        }

        if (loginState.ExpiresAt < DateTime.UtcNow)
        {
            ErrorMessage = "Login link has expired. Please request a new one from the bot.";
            return Page();
        }

        // Get the authenticated user
        var user = await userManager.GetUserAsync(User);
        if (user is null || user.TenantId is null)
        {
            ErrorMessage = "Unable to resolve your account. Please ensure you belong to a company.";
            return Page();
        }

        // Resolve tenant
        var tenant = await masterUow.Repository<Tenant>()
            .GetAsync(t => t.Id == user.TenantId.Value, ct);

        if (tenant is null)
        {
            ErrorMessage = "Unable to resolve your company.";
            return Page();
        }

        // Resolve employee role
        tenantUow.SetCurrentTenant(tenant);
        var employee = await tenantUow.Repository<Employee>()
            .GetByIdAsync(user.Id, ct);

        var roleName = employee?.Role?.Name;
        var chatRole = ResolveChatRole(roleName);

        // For group chats, require Owner/Admin/Manager role
        if (loginState.ChatType == TelegramChatType.Group)
        {
            if (chatRole != TelegramChatRole.Dispatcher)
            {
                ErrorMessage = "Only owners, managers, or dispatchers can connect group chats.";
                return Page();
            }
        }

        // Create or update TelegramChat entity
        var existingChat = await tenantUow.Repository<TelegramChat>()
            .GetAsync(c => c.ChatId == loginState.ChatId, ct);

        if (existingChat is not null)
        {
            existingChat.UserId = user.Id;
            existingChat.ChatType = loginState.ChatType;
            existingChat.Role = loginState.ChatType == TelegramChatType.Group ? null : chatRole;
            existingChat.FirstName = user.FirstName;
            existingChat.GroupTitle = loginState.ChatType == TelegramChatType.Group ? null : null;
            existingChat.ConnectedAt = DateTime.UtcNow;
        }
        else
        {
            var newChat = new TelegramChat
            {
                ChatId = loginState.ChatId,
                ChatType = loginState.ChatType,
                Role = loginState.ChatType == TelegramChatType.Group ? null : chatRole,
                UserId = user.Id,
                FirstName = user.FirstName
            };
            await tenantUow.Repository<TelegramChat>().AddAsync(newChat, ct);
        }

        await tenantUow.SaveChangesAsync(ct);

        // Mark the login state as consumed with results
        loginState.IsConsumed = true;
        loginState.UserId = user.Id;
        loginState.TenantId = tenant.Id;
        loginState.UserDisplayName = user.GetFullName();
        loginState.TenantName = tenant.Name;
        loginState.ResolvedRole = loginState.ChatType == TelegramChatType.Group ? null : chatRole;
        await masterUow.SaveChangesAsync(ct);

        // Show success page
        Success = true;
        TenantName = tenant.Name;
        UserName = user.GetFullName();
        RoleDisplay = chatRole?.ToString();
        return Page();
    }

    private static TelegramChatRole? ResolveChatRole(string? roleName)
    {
        return roleName switch
        {
            TenantRoles.Driver => TelegramChatRole.Driver,
            TenantRoles.Owner or TenantRoles.Manager or TenantRoles.Dispatcher => TelegramChatRole.Dispatcher,
            _ => null
        };
    }
}
