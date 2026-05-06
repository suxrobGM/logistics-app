using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Tax.Data;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Seeds demo tenant VAT rates in the master database so manual invoice tax calculation has
/// explicit jurisdiction rows for the EU demo tenant.
/// </summary>
internal sealed class TenantTaxRateSeeder(ILogger<TenantTaxRateSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(TenantTaxRateSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 70;
    public override bool IsTenantScoped => true;
    public override IReadOnlyList<string> DependsOn => [nameof(DemoTenantsSeeder)];

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        var tenant = context.CurrentTenant;
        if (tenant is null || context.Region?.Region != Region.EU)
        {
            return;
        }

        LogStarting();

        var repo = context.MasterUnitOfWork.Repository<TenantTaxRate>();
        var existingRates = await repo.GetListAsync(r => r.TenantId == tenant.Id, cancellationToken);
        var effectiveFrom = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var added = 0;

        foreach (var (countryCode, ratePercent) in EuVatRates.StandardRates
                     .Where(r => EuVatRules.IsEuMember(r.Key)))
        {
            var countryRateExists = existingRates.Any(r =>
                string.IsNullOrEmpty(r.Jurisdiction.Region) &&
                r.Jurisdiction.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase));

            if (countryRateExists)
            {
                continue;
            }

            await repo.AddAsync(new TenantTaxRate
            {
                TenantId = tenant.Id,
                Jurisdiction = TaxJurisdiction.ForCountry(countryCode),
                RatePercent = ratePercent,
                Description = $"Standard VAT {ratePercent:0.##}% ({countryCode})",
                EffectiveFrom = effectiveFrom
            }, cancellationToken);

            added++;
        }

        if (added > 0)
        {
            await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        LogCompleted(added);
    }
}
