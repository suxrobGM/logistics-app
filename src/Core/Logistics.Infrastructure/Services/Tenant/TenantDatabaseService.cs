using System.Data.Common;
using System.Text.RegularExpressions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Infrastructure.Data;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using CustomClaimTypes = Logistics.Shared.Identity.Claims.CustomClaimTypes;

namespace Logistics.Infrastructure.Services;

public partial class TenantDatabaseService(
    TenantDbContext context,
    TenantsDatabaseOptions databaseOptions,
    ILogger<TenantDatabaseService> logger)
    : ITenantDatabaseService
{
    public string GenerateConnectionString(string tenantName)
    {
        if (string.IsNullOrEmpty(databaseOptions.DatabaseNameTemplate))
        {
            throw new InvalidOperationException(
                "The database name template is not defined in the TenantsDatabaseOptions appsettings.json file");
        }

        var databaseName = TenantDatabaseNameRegex().Replace(databaseOptions.DatabaseNameTemplate, tenantName);
        var connectionString =
            $"Host={databaseOptions.DatabaseHost}; Database={databaseName}; Port=5432; Username={databaseOptions.DatabaseUserId}; Password={databaseOptions.DatabasePassword}";
        return connectionString;
    }

    public async Task<bool> CreateDatabaseAsync(string connectionString)
    {
        try
        {
            context.Database.SetConnectionString(connectionString);
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

    public async Task<bool> DeleteDatabaseAsync(string connectionString)
    {
        try
        {
            // Build a main connection for the database to drop
            var sourceBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var targetDbName = sourceBuilder.Database;

            // Switch to a different database like 'postgres' to perform the drop
            var masterBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = sourceBuilder.Host,
                Port = sourceBuilder.Port,
                Username = sourceBuilder.Username,
                Password = sourceBuilder.Password,
                Database = "postgres"
            };

            await using var connection = new NpgsqlConnection(masterBuilder.ConnectionString);
            await connection.OpenAsync();

            var dropQuery = $"DROP DATABASE {targetDbName}";
            await using var cmd = new NpgsqlCommand(dropQuery, connection);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (DbException ex)
        {
            logger.LogError("Thrown exception in TenantDatabaseService.DeleteDatabaseAsync(): {@Exception}", ex);
            return false;
        }
    }

    private async Task AddTenantRoles()
    {
        var roles = TenantRoles.GetValues();

        foreach (var tenantRole in roles)
        {
            var role = new TenantRole(tenantRole.Value)
            {
                DisplayName = tenantRole.DisplayName
            };

            var existingRole = await context.Set<TenantRole>()
                .Include(r => r.Claims)
                .FirstOrDefaultAsync(i => i.Name == role.Name);

            if (existingRole != null)
            {
                // Sync role claims - add missing and remove stale
                await SyncRolePermissionsAsync(existingRole);
                context.Set<TenantRole>().Update(existingRole);
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
                role.Claims.Add(new TenantRoleClaim(CustomClaimTypes.Permission, permission));
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
            logger.LogInformation("Removed permission '{Permission}' from tenant role '{Role}'", claim.ClaimValue, role.Name);
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
