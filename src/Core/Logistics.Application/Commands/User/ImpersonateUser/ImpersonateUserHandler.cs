using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Commands;

internal sealed class ImpersonateUserHandler(
    IMasterUnitOfWork masterUow,
    UserManager<User> userManager,
    ICurrentUserService currentUserService,
    IHttpContextAccessor httpContextAccessor,
    IOptions<ImpersonationOptions> impersonationOptions,
    IOptions<IdentityServerOptions> identityServerOptions,
    ILogger<ImpersonateUserHandler> logger)
    : IAppRequestHandler<ImpersonateUserCommand, Result<ImpersonateUserResult>>
{
    public async Task<Result<ImpersonateUserResult>> Handle(
        ImpersonateUserCommand req, CancellationToken ct)
    {
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown";

        // 1. Get current admin user
        var adminUserId = currentUserService.GetUserId();
        if (adminUserId is null)
        {
            await LogFailedAttemptAsync(null, null, req.TargetEmail, "User not authenticated", ipAddress, userAgent, ct);
            return Result<ImpersonateUserResult>.Fail("User not authenticated.");
        }

        var adminUser = await userManager.FindByIdAsync(adminUserId.Value.ToString());
        if (adminUser is null)
        {
            await LogFailedAttemptAsync(adminUserId, null, req.TargetEmail, "Admin user not found", ipAddress, userAgent, ct);
            return Result<ImpersonateUserResult>.Fail("Admin user not found.");
        }

        // 2. Validate caller is SuperAdmin or Admin
        var adminRoles = await userManager.GetRolesAsync(adminUser);
        if (!adminRoles.Contains(AppRoles.SuperAdmin) && !adminRoles.Contains(AppRoles.Admin))
        {
            await LogFailedAttemptAsync(adminUserId, adminUser.Email, req.TargetEmail, "User lacks required role", ipAddress, userAgent, ct);
            return Result<ImpersonateUserResult>.Fail("Only SuperAdmin or Admin can impersonate users.");
        }

        // 3. Validate master password
        if (req.MasterPassword != impersonationOptions.Value.MasterPassword)
        {
            await LogFailedAttemptAsync(adminUserId, adminUser.Email, req.TargetEmail, "Invalid master password", ipAddress, userAgent, ct);
            return Result<ImpersonateUserResult>.Fail("Invalid master password.");
        }

        // 4. Find target user
        var targetUser = await userManager.FindByEmailAsync(req.TargetEmail);
        if (targetUser is null)
        {
            await LogFailedAttemptAsync(adminUserId, adminUser.Email, req.TargetEmail, "Target user not found", ipAddress, userAgent, ct);
            return Result<ImpersonateUserResult>.Fail("Target user not found.");
        }

        // 5. Create impersonation token (one-time use, short expiry)
        var expirationMinutes = impersonationOptions.Value.TokenExpirationMinutes > 0
            ? impersonationOptions.Value.TokenExpirationMinutes
            : 5;

        var impersonationToken = new ImpersonationToken
        {
            Token = TokenGenerator.GenerateSecureToken(64),
            AdminUserId = adminUserId.Value,
            TargetUserId = targetUser.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            IsUsed = false
        };

        await masterUow.Repository<ImpersonationToken>().AddAsync(impersonationToken, ct);

        // 6. Create audit log entry
        await LogSuccessfulAttemptAsync(adminUserId.Value, adminUser.Email!, req.TargetEmail, targetUser.Id, ipAddress, userAgent, ct);
        await masterUow.SaveChangesAsync(ct);

        // 7. Build impersonation URL
        var impersonationUrl = $"{identityServerOptions.Value.Authority}/Account/Impersonate?token={impersonationToken.Token}";

        return Result<ImpersonateUserResult>.Ok(new ImpersonateUserResult
        {
            ImpersonationToken = impersonationToken.Token,
            ImpersonationUrl = impersonationUrl
        });
    }

    private async Task LogSuccessfulAttemptAsync(
        Guid adminUserId,
        string adminEmail,
        string targetEmail,
        Guid targetUserId,
        string ipAddress,
        string userAgent,
        CancellationToken ct)
    {
        var auditLog = new ImpersonationAuditLog
        {
            AdminUserId = adminUserId,
            AdminEmail = adminEmail,
            TargetUserId = targetUserId,
            TargetEmail = targetEmail,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            WasSuccessful = true
        };

        await masterUow.Repository<ImpersonationAuditLog>().AddAsync(auditLog, ct);

        logger.LogInformation(
            "IMPERSONATION SUCCESS: Admin {AdminEmail} successfully impersonated {TargetEmail} from IP {IpAddress}",
            adminEmail, targetEmail, ipAddress);
    }

    private async Task LogFailedAttemptAsync(
        Guid? adminUserId,
        string? adminEmail,
        string targetEmail,
        string failureReason,
        string ipAddress,
        string userAgent,
        CancellationToken ct)
    {
        var auditLog = new ImpersonationAuditLog
        {
            AdminUserId = adminUserId ?? Guid.Empty,
            AdminEmail = adminEmail ?? "unknown",
            TargetUserId = Guid.Empty,
            TargetEmail = targetEmail,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            WasSuccessful = false,
            FailureReason = failureReason
        };

        await masterUow.Repository<ImpersonationAuditLog>().AddAsync(auditLog, ct);
        await masterUow.SaveChangesAsync(ct);

        logger.LogWarning(
            "IMPERSONATION FAILED: Admin {AdminEmail} attempted to impersonate {TargetEmail} from IP {IpAddress}. Reason: {FailureReason}",
            adminEmail ?? "unknown", targetEmail, ipAddress, failureReason);
    }
}
