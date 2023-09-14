using System.Data.Common;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Logistics.Domain.Enums;
using Microsoft.Extensions.Logging;
using Logistics.Shared.Policies;
using Microsoft.Data.SqlClient;
using CustomClaimTypes = Logistics.Shared.Claims.CustomClaimTypes;

namespace Logistics.Infrastructure.EF.Services;

public class TenantDatabaseService : ITenantDatabaseService
{
    private readonly TenantDbContext _context;
    private readonly TenantsDatabaseOptions _databaseOptions;
    private readonly ILogger<TenantDatabaseService> _logger;

    public TenantDatabaseService(
        TenantDbContext context,
        TenantsDatabaseOptions databaseOptions,
        ILogger<TenantDatabaseService> logger)
    {
        _context = context;
        _logger = logger;
        _databaseOptions = databaseOptions;
    }

    public string GenerateConnectionString(string tenantName)
    {
        if (string.IsNullOrEmpty(_databaseOptions.DatabaseNameTemplate))
        {
            throw new InvalidOperationException(
                "The database name template is not defined in the TenantsDatabaseOptions appsettings.json file");
        }
        
        var databaseName = Regex.Replace(_databaseOptions.DatabaseNameTemplate, "{tenant}", tenantName);
        var connectionString =  $"Server={_databaseOptions.DatabaseHost}; Database={databaseName}; Uid={_databaseOptions.DatabaseUserId}; Pwd={_databaseOptions.DatabasePassword}; TrustServerCertificate=true";
        return connectionString;
    }

    public async Task<bool> CreateDatabaseAsync(string connectionString)
    {
        try
        {
            await _context.Database.MigrateAsync();
            await AddTenantRoles();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Thrown exception in TenantDatabaseService.CreateDatabaseAsync(): {Exception}", ex);
            return false;
        }
    }

    public async Task<bool> DeleteDatabaseAsync(string connectionString)
    {
        try
        {
            var connection = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            var database = connection["Initial Catalog"];
            var dropQuery = $"DROP DATABASE '{database}'";
            await using var sqlCommand = new SqlCommand(dropQuery);
            await sqlCommand.ExecuteScalarAsync();
            return true;
        }
        catch (DbException ex)
        {
            _logger.LogError("Thrown exception in TenantDatabaseService.DeleteDatabaseAsync(): {@Exception}", ex);
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

            var existingRole = await _context.Set<TenantRole>().FirstOrDefaultAsync(i => i.Name == role.Name);
            if (existingRole != null)
                continue;
            
            AddPermissions(role, TenantRolePermissions.GetBasicPermissions());
            
            switch (role.Name)
            {
                case TenantRoles.Owner:
                    AddPermissions(role, TenantRolePermissions.Owner);
                    break;
                case TenantRoles.Manager:
                    AddPermissions(role, TenantRolePermissions.Manager);
                    break;
                case TenantRoles.Dispatcher:
                    AddPermissions(role, TenantRolePermissions.Dispatcher);
                    break;
                case TenantRoles.Driver:
                    AddPermissions(role, TenantRolePermissions.Driver);
                    break;
            }

            _context.Set<TenantRole>().Add(role);
            _logger.LogInformation("Added tenant role '{Role}'", role.Name);
        }

        await _context.SaveChangesAsync();
    }

    private void AddPermissions(TenantRole role, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            var claim = new Claim(CustomClaimTypes.Permission, permission);
            role.Claims.Add(new TenantRoleClaim(claim.Type, claim.Value));
            _logger.LogInformation("Added claim '{ClaimType}' - '{ClaimValue}' to the tenant role '{Role}'", claim.Type, claim.Value, role.Name);
        }
    }
}
