using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Conventions;
using Logistics.Infrastructure.Persistence.Extensions;
using Logistics.Infrastructure.Persistence.Helpers;
using Logistics.Infrastructure.Persistence.Interceptors;
using Logistics.Infrastructure.Persistence.Options;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Persistence.Data;

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
    private readonly AuditableEntitySaveChangesInterceptor? auditableEntity;
    private readonly string connectionString;
    private readonly DispatchDomainEventsInterceptor? dispatchDomain;
    private readonly ILogger<MasterDbContext>? logger;

    public MasterDbContext(
        MasterDbContextOptions options,
        DispatchDomainEventsInterceptor? dispatchDomain = null,
        AuditableEntitySaveChangesInterceptor? auditableEntity = null,
        ILogger<MasterDbContext>? logger = null)
    {
        this.dispatchDomain = dispatchDomain;
        this.auditableEntity = auditableEntity;
        connectionString = options.ConnectionString ?? ConnectionStrings.LocalMaster;
        this.logger = logger;
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (dispatchDomain is not null)
        {
            options.AddInterceptors(dispatchDomain);
        }

        if (auditableEntity is not null)
        {
            options.AddInterceptors(auditableEntity);
        }

        if (!options.IsConfigured)
        {
            DbContextHelpers.ConfigurePostgreSql(connectionString, options);
            logger?.LogInformation("Configured master database with connection string: {ConnectionString}",
                connectionString);
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new SnakeCaseEnumConvention());
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Issue with EF Core 10: Temporary solution to ignore IdentityPasskeyData entity
        // until proper Passkey support is implemented
        builder.Ignore<IdentityPasskeyData>();

        // Scan and apply all configurations from the /Data/Configurations folder
        // for entities implementing IMasterEntity
        builder.ApplyMasterConfigurationsFromAssembly<MasterDbContext>();

        // Prune entity types that are only relevant for the master database
        // It avoids issues with tenant-specific entities being included in the master context and migration errors
        builder.PruneTenantOnlyTypesForMaster();
    }
}
