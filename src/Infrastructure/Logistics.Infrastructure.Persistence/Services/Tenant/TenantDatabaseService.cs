using System.Data.Common;
using System.Text.RegularExpressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Infrastructure.Persistence.Data;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Logistics.Application.Abstractions.Tenancy;
using CustomClaimTypes = Logistics.Shared.Identity.Claims.CustomClaimTypes;

namespace Logistics.Infrastructure.Persistence.Services;

internal partial class TenantDatabaseService(
    TenantDbContext context,
    TenantDatabaseDefaults databaseOptions,
    ILogger<TenantDatabaseService> logger)
    : ITenantDatabaseService
{
    public string GenerateDatabaseName(string tenantName)
    {
        if (string.IsNullOrEmpty(databaseOptions.NameTemplate))
        {
            throw new InvalidOperationException(
                "The NameTemplate is not defined in the TenantDatabaseDefaults appsettings.json section");
        }

        return TenantDatabaseNameRegex().Replace(databaseOptions.NameTemplate, tenantName);
    }

    public async Task<bool> CreateDatabaseAsync(string databaseName)
    {
        try
        {
            if (await DatabaseExistsAsync(databaseName))
            {
                logger.LogWarning("Tenant database '{DatabaseName}' already exists; refusing to provision into it", databaseName);
                return false;
            }

            context.Database.SetConnectionString(databaseOptions.BuildConnectionString(databaseName));
            await context.Database.MigrateAsync();
            await AddTenantRoles();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Thrown exception in TenantDatabaseService.CreateDatabaseAsync(): {Exception}", ex);
            return false;
        }
    }

    public async Task<bool> DeleteDatabaseAsync(string databaseName)
    {
        try
        {
            await using var connection = new NpgsqlConnection(databaseOptions.BuildConnectionString("postgres"));
            await connection.OpenAsync();

            // Terminate all active connections to the target database before dropping
            await using (var terminateCmd = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @name AND pid <> pg_backend_pid()",
                connection))
            {
                terminateCmd.Parameters.AddWithValue("name", databaseName);
                await terminateCmd.ExecuteNonQueryAsync();
            }

            await using var dropCmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\"", connection);
            await dropCmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (DbException ex)
        {
            logger.LogError("Thrown exception in TenantDatabaseService.DeleteDatabaseAsync(): {@Exception}", ex);
            return false;
        }
    }

    private async Task<bool> DatabaseExistsAsync(string databaseName)
    {
        await using var connection = new NpgsqlConnection(databaseOptions.BuildConnectionString("postgres"));
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection);
        command.Parameters.AddWithValue("name", databaseName);
        return await command.ExecuteScalarAsync() is not null;
    }

    private async Task AddTenantRoles()
    {
        var roles = TenantRoles.GetValues();

        foreach (var tenantRole in roles)
        {
            var role = new TenantRole(tenantRole.Value) { DisplayName = tenantRole.DisplayName };

            var existingRole = await context.Set<TenantRole>()
                .Include(r => r.Claims)
                .FirstOrDefaultAsync(i => i.Name == role.Name);

            if (existingRole != null)
            {
                // existingRole is tracked, so the change tracker handles claim add/remove.
                // Don't call Update(): client-generated keys make it mark new claims as
                // Modified, issuing UPDATEs for non-existent rows (DbUpdateConcurrencyException).
                await SyncRolePermissionsAsync(existingRole);
                logger.LogInformation("Updated tenant role '{Role}'", existingRole.Name);
            }
            else
            {
                // Add new role and its claims
                var basicPermissions = TenantRolePermissions.GetBasicPermissions();
                var rolePermissions = TenantRolePermissions.GetPermissionsForRole(role.Name);
                AddRolePermissions(role, basicPermissions.Concat(rolePermissions).Distinct());

                context.Set<TenantRole>().Add(role);
                logger.LogInformation("Added tenant role '{Role}'", role.Name);
            }
        }

        await context.SaveChangesAsync();
    }

    private Task SyncRolePermissionsAsync(TenantRole role)
    {
        var requiredPermissions = TenantRolePermissions.GetPermissionsForRole(role.Name)
            .Concat(TenantRolePermissions.GetBasicPermissions())
            .Distinct()
            .ToList();

        // Add missing permissions
        foreach (var permission in requiredPermissions)
        {
            if (!role.Claims.Any(c => c.ClaimType == CustomClaimTypes.Permission && c.ClaimValue == permission))
            {
                var claim = new TenantRoleClaim(CustomClaimTypes.Permission, permission);
                role.Claims.Add(claim);
                // Entity presets a client-generated Guid key, so a claim discovered only via the
                // tracked collection is marked Modified (UPDATE on a non-existent row →
                // DbUpdateConcurrencyException). Add() pins it as an insert.
                context.Set<TenantRoleClaim>().Add(claim);
                logger.LogInformation("Added permission '{Permission}' to tenant role '{Role}'", permission, role.Name);
            }
        }

        // Remove permissions that are no longer in the required list
        var claimsToRemove = role.Claims
            .Where(c => c.ClaimType == CustomClaimTypes.Permission && !requiredPermissions.Contains(c.ClaimValue!))
            .ToList();

        foreach (var claim in claimsToRemove)
        {
            role.Claims.Remove(claim);
            logger.LogInformation("Removed permission '{Permission}' from tenant role '{Role}'", claim.ClaimValue,
                role.Name);
        }

        return Task.CompletedTask;
    }

    private void AddRolePermissions(TenantRole role, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            // Add the claim only if it does not already exist.
            if (!role.Claims.Any(c => c.ClaimType == CustomClaimTypes.Permission && c.ClaimValue == permission))
            {
                role.Claims.Add(new TenantRoleClaim(CustomClaimTypes.Permission, permission));

                logger.LogInformation("Added claim '{ClaimType}' - '{ClaimValue}' to the tenant role '{Role}'",
                    CustomClaimTypes.Permission, permission, role.Name);
            }
        }
    }

    [GeneratedRegex("{tenant}")]
    private static partial Regex TenantDatabaseNameRegex();
}
