using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Regions;
using Logistics.DbMigrator.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Persistence.Options;
using Microsoft.AspNetCore.Identity;

namespace Logistics.DbMigrator.Workers;

/// <summary>
/// Orchestrates seeders across one or more demo tenants.
///
/// Pass 1 — global Infrastructure seeders run once against master DB
///          (roles, super admin, plans, Stripe, DemoTenantsSeeder which provisions tenants).
/// Pass 2 — for each demo tenant, mutate the singleton TenantDbContextOptions.ConnectionString,
///          open a fresh DI scope (which produces a TenantDbContext bound to that tenant's DB),
///          and run the tenant-scoped seeders (Infrastructure-tenant-scoped + all FakeData).
/// </summary>
internal class SeederOrchestrationWorker(
    ILogger<SeederOrchestrationWorker> logger,
    IServiceScopeFactory scopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var rootScope = scopeFactory.CreateScope();
        var configuration = rootScope.ServiceProvider.GetRequiredService<IConfiguration>();

        var allSeeders = rootScope.ServiceProvider.GetServices<ISeeder>();
        var orderedSeeders = SeederRegistry.GetOrderedSeeders(allSeeders, logger);

        var globalSeeders = orderedSeeders.Where(s => !s.IsTenantScoped).ToList();
        var tenantScopedSeederTypes = orderedSeeders
            .Where(s => s.IsTenantScoped)
            .Select(s => s.GetType())
            .ToList();

        // ─── Pass 1: global Infrastructure seeders against master DB ────────────────
        var rootContext = BuildContext(rootScope, configuration);
        logger.LogInformation("Running {Count} global seeder(s)", globalSeeders.Count);
        foreach (var seeder in globalSeeders)
        {
            await RunSeederAsync(seeder, rootContext, cancellationToken);
        }

        // ─── Pass 2: per-tenant loop ─────────────────────────────────────────────────
        var tenantConfigs = configuration.GetSection("Tenants").Get<List<DemoTenantConfig>>() ?? [];
        if (tenantConfigs.Count == 0)
        {
            logger.LogWarning("No tenants configured; skipping tenant-scoped seeders");
            return;
        }

        var masterRepo = rootScope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>().Repository<Tenant>();
        var tenantOptions = rootScope.ServiceProvider.GetRequiredService<TenantDbContextOptions>();
        var regionFactory = rootScope.ServiceProvider.GetRequiredService<IRegionProfileFactory>();

        foreach (var tenantConfig in tenantConfigs)
        {
            var tenantEntity = await masterRepo.GetAsync(t => t.Name == tenantConfig.Name, cancellationToken);
            if (tenantEntity is null)
            {
                logger.LogError("Tenant '{Name}' missing from master DB after DemoTenantsSeeder", tenantConfig.Name);
                continue;
            }

            // Switch the singleton options so the next-built TenantDbContext targets this tenant.
            // Source the connection string from the persisted Tenant entity, which holds whatever
            // DemoTenantsSeeder resolved (explicit config or the WithReference fallback).
            tenantOptions.ConnectionString = tenantEntity.ConnectionString;

            using var tenantScope = scopeFactory.CreateScope();
            var tenantContext = BuildContext(tenantScope, configuration);
            tenantContext.CurrentTenant = tenantEntity;
            tenantContext.Region = regionFactory.Get(tenantConfig.Region);

            logger.LogInformation("─── Seeding tenant '{Name}' ({Region}) ───",
                tenantEntity.Name, tenantConfig.Region);

            // Resolve seeder instances from the tenant scope so they pick up scoped deps.
            var tenantSeeders = tenantScope.ServiceProvider.GetServices<ISeeder>().ToList();
            var tenantSeederMap = tenantSeeders.ToDictionary(s => s.GetType());

            foreach (var seederType in tenantScopedSeederTypes)
            {
                if (!tenantSeederMap.TryGetValue(seederType, out var seeder))
                {
                    continue;
                }
                await RunSeederAsync(seeder, tenantContext, cancellationToken);
            }
        }

        logger.LogInformation("Database seeding completed successfully");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static SeederContext BuildContext(IServiceScope scope, IConfiguration configuration)
    {
        return new SeederContext
        {
            ServiceProvider = scope.ServiceProvider,
            Configuration = configuration,
            MasterUnitOfWork = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>(),
            TenantUnitOfWork = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>(),
            UserManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>()
        };
    }

    private async Task RunSeederAsync(ISeeder seeder, SeederContext context, CancellationToken ct)
    {
        if (await seeder.ShouldSkipAsync(context, ct))
        {
            logger.LogInformation("Skipping {SeederName} (data already exists)", seeder.Name);
            return;
        }

        try
        {
            await seeder.SeedAsync(context, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {SeederName}", seeder.Name);
            throw;
        }
    }
}
