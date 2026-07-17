using Logistics.Application.Modules.Integrations.FuelCards.Services;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds a Demo fuel card provider configuration and runs one sync against it, producing
/// ~3 months of deterministic transactions: most auto-matched to seeded trucks (materialized
/// as Paid fuel expenses with purchase jurisdictions), the rest pending in the review queue.
/// </summary>
internal class FuelCardSeeder(ILogger<FuelCardSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(FuelCardSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 170;
    public override IReadOnlyList<string> DependsOn => [nameof(TruckSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<FuelCardProviderConfiguration>()
            .CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var config = new FuelCardProviderConfiguration
        {
            ProviderType = FuelCardProviderType.Demo,
            ApiKey = "demo-fuel-card-key",
            IsActive = true
        };

        await context.TenantUnitOfWork.Repository<FuelCardProviderConfiguration>()
            .AddAsync(config, cancellationToken);
        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);

        // The Demo provider generates deterministic transactions from the tenant's trucks;
        // running the real sync exercises the same code path production uses.
        var syncService = context.ServiceProvider.GetRequiredService<IFuelCardSyncService>();
        var result = await syncService.SyncCurrentTenantAsync(FuelCardProviderType.Demo, cancellationToken);

        logger.LogInformation(
            "Seeded Demo fuel card provider: {Imported} transactions ({Matched} matched, {Pending} pending)",
            result.Imported, result.Matched, result.Pending);
        LogCompleted(result.Imported);
    }
}
