using System.Text.Json;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Seeds master-DB IFTA fuel tax rates from SeedData/ifta-tax-rates.json (approximate values;
/// the official matrix changes quarterly and is a recurring ops task). Idempotent upsert per
/// (year, quarter, jurisdiction).
/// </summary>
internal sealed class IftaTaxRateSeeder(ILogger<IftaTaxRateSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(IftaTaxRateSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 75;

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "ifta-tax-rates.json");
        if (!File.Exists(path))
        {
            logger.LogWarning("IFTA rate seed file not found at {Path}; skipping", path);
            return;
        }

        LogStarting();

        var seed = JsonSerializer.Deserialize<IftaRateSeedFile>(
            await File.ReadAllTextAsync(path, cancellationToken),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (seed?.Quarters is null || seed.Rates is null)
        {
            logger.LogWarning("IFTA rate seed file is empty or malformed; skipping");
            return;
        }

        var repo = context.MasterUnitOfWork.Repository<IftaTaxRate>();
        var added = 0;

        foreach (var quarterDef in seed.Quarters)
        {
            var existing = await repo.GetListAsync(
                r => r.Year == quarterDef.Year && r.Quarter == quarterDef.Quarter, cancellationToken);
            var existingKeys = existing
                .Select(r => (r.Jurisdiction.CountryCode, r.Jurisdiction.Region ?? string.Empty))
                .ToHashSet();

            foreach (var rate in seed.Rates)
            {
                if (rate.Country is null || rate.Region is null ||
                    existingKeys.Contains((rate.Country, rate.Region)))
                {
                    continue;
                }

                await repo.AddAsync(new IftaTaxRate
                {
                    Jurisdiction = new TaxJurisdiction { CountryCode = rate.Country, Region = rate.Region },
                    Year = quarterDef.Year,
                    Quarter = quarterDef.Quarter,
                    RatePerGallon = rate.RatePerGallon,
                    SurchargeRatePerGallon = rate.SurchargePerGallon
                }, cancellationToken);
                added++;
            }
        }

        if (added > 0)
        {
            await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        LogCompleted(added);
    }

    private sealed record IftaRateSeedFile
    {
        public List<QuarterDef>? Quarters { get; init; }
        public List<RateDef>? Rates { get; init; }
    }

    private sealed record QuarterDef(int Year, int Quarter);

    private sealed record RateDef
    {
        public string? Country { get; init; }
        public string? Region { get; init; }
        public decimal RatePerGallon { get; init; }
        public decimal? SurchargePerGallon { get; init; }
    }
}
