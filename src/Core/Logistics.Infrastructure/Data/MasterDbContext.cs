using Logistics.Domain.Entities;
using Logistics.Infrastructure.Data.Extensions;
using Logistics.Infrastructure.Helpers;
using Logistics.Infrastructure.Interceptors;
using Logistics.Infrastructure.Options;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Data;

public class MasterDbContext : IdentityDbContext<
        User,
        AppRole,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        AppRoleClaim,
        IdentityUserToken<Guid>>,
    IDataProtectionKeyContext
{
    private readonly AuditableEntitySaveChangesInterceptor? _auditableEntity;
    private readonly string _connectionString;
    private readonly DispatchDomainEventsInterceptor? _dispatchDomain;
    private readonly ILogger<MasterDbContext>? _logger;

    public MasterDbContext(
        MasterDbContextOptions options,
        DispatchDomainEventsInterceptor? dispatchDomain = null,
        AuditableEntitySaveChangesInterceptor? auditableEntity = null,
        ILogger<MasterDbContext>? logger = null)
    {
        _dispatchDomain = dispatchDomain;
        _auditableEntity = auditableEntity;
        _connectionString = options.ConnectionString ?? ConnectionStrings.LocalMaster;
        _logger = logger;
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (_dispatchDomain is not null)
        {
            options.AddInterceptors(_dispatchDomain);
        }

        if (_auditableEntity is not null)
        {
            options.AddInterceptors(_auditableEntity);
        }

        if (!options.IsConfigured)
        {
            DbContextHelpers.ConfigurePostgreSql(_connectionString, options);
            _logger?.LogInformation("Configured master database with connection string: {ConnectionString}",
                _connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Scan and apply all configurations from the /Data/Configurations folder
        // for entities implementing IMasterEntity
        builder.ApplyMasterConfigurationsFromAssemblyContaining<MasterDbContext>();

        // Prune entity types that are only relevant for the master database
        // It avoids issues with tenant-specific entities being included in the master context and migration errors
        builder.PruneTenantOnlyTypesForMaster();
    }
}
