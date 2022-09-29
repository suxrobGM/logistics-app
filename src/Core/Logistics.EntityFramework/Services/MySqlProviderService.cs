using System.Data.Common;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using ClaimTypes = Logistics.Domain.Shared.ClaimTypes;

namespace Logistics.EntityFramework.Services;

public class MySqlProviderService : IDatabaseProviderService
{
    private readonly TenantDbContext _context;
    private readonly TenantsSettings _settings;
    private readonly ILogger<MySqlProviderService> _logger;

    public MySqlProviderService(
        TenantDbContext context,
        TenantsSettings settings,
        ILogger<MySqlProviderService> logger)
    {
        _context = context;
        _logger = logger;
        _settings = settings;
    }

    public string GenerateConnectionString(string databaseName)
    {
        return $"Server={_settings.DatabaseHost}; Database={databaseName}; Uid={_settings.DatabaseUserId}; Pwd={_settings.DatabasePassword}";
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
            _logger.LogError("Thrown exception in MySqlProviderService.CreateDatabaseAsync(): {Exception}", ex);
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
            await using var mySqlCommand = new MySqlCommand(dropQuery);
            await mySqlCommand.ExecuteScalarAsync();
            return true;
        }
        catch (DbException ex)
        {
            _logger.LogError("Thrown exception in MySqlProviderService.DeleteDatabaseAsync(): {@Exception}", ex);
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
            
            AddBasicPermissions(role);
            
            switch (role.Name)
            {
                case TenantRoles.Owner:
                    AddPermissions(role, "Employee");
                    AddPermissions(role, "Load");
                    AddPermissions(role, "Truck");
                    AddPermissions(role, "TenantRole");
                    AddPermission(role, Permissions.Report.View);
                    break;
                case TenantRoles.Manager:
                    AddPermissions(role, "Load");
                    AddPermissions(role, "Truck");
                    AddPermission(role, Permissions.Employee.Create);
                    AddPermission(role, Permissions.Employee.Edit);
                    AddPermission(role, Permissions.Report.View);
                    break;
                case TenantRoles.Dispatcher:
                    AddPermissions(role, "Load");
                    AddPermission(role, Permissions.Truck.View);
                    break;
            }

            _context.Set<TenantRole>().Add(role);
            _logger.LogInformation("Added tenant role '{Role}'", role.Name);
        }

        await _context.SaveChangesAsync();
    }
    
    private void AddBasicPermissions(TenantRole role)
    {
        AddPermission(role, Permissions.AppRole.View);
        AddPermission(role, Permissions.TenantRole.View);
        AddPermission(role, Permissions.User.View);
        AddPermission(role, Permissions.Employee.View);
    }
    
    private void AddPermissions(TenantRole role, string module)
    {
        var permissions = Permissions.GeneratePermissions(module);
        foreach (var permission in permissions)
        {
            AddPermission(role, permission);
        }
    }
    
    private void AddPermission(TenantRole role, string permission)
    {
        var claim = new Claim(ClaimTypes.Permission, permission);
        role.Claims.Add(TenantRoleClaim.FromClaim(claim));
        _logger.LogInformation("Added claim '{ClaimType}' - '{ClaimValue}' to the tenant role '{Role}'", claim.Value, claim.Type, role.Name);
    }
}
