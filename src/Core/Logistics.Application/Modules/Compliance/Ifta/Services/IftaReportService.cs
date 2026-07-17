using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

internal sealed class IftaReportService(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow) : IIftaReportService
{
    private const decimal LitersPerGallon = 3.785411784m;

    public async Task<IftaReportDto> GetReportAsync(int year, int quarter, CancellationToken ct = default)
    {
        var snapshot = await tenantUow.Repository<IftaQuarterSnapshot>()
            .GetAsync(s => s.Year == year && s.Quarter == quarter, ct);

        if (snapshot is not null)
        {
            var report = JsonSerializer.Deserialize<IftaReportDto>(snapshot.ReportJson);
            if (report is not null)
            {
                return report;
            }
        }

        return await BuildReportAsync(year, quarter, ct);
    }

    public async Task<IftaReportDto> BuildReportAsync(int year, int quarter, CancellationToken ct = default)
    {
        var start = IftaQuarters.StartOf(year, quarter);
        var end = IftaQuarters.EndOf(year, quarter);
        var startDate = DateOnly.FromDateTime(start);
        var endDate = DateOnly.FromDateTime(end);

        // Miles per jurisdiction from the daily rollups
        var mileage = await tenantUow.Repository<TruckJurisdictionMileage>().Query()
            .Where(m => m.Date >= startDate && m.Date < endDate)
            .GroupBy(m => new { m.Jurisdiction.CountryCode, m.Jurisdiction.Region })
            .Select(g => new { g.Key.CountryCode, g.Key.Region, Miles = g.Sum(m => m.Miles) })
            .ToListAsync(ct);

        // Fuel purchases in the quarter (gallons); jurisdiction may be missing on manual entries
        var fuelPurchases = await tenantUow.Repository<Expense>().Query()
            .OfType<TruckExpense>()
            .Where(e => e.Category == TruckExpenseCategory.Fuel
                && e.Status != ExpenseStatus.Rejected
                && e.Quantity != null
                && e.ExpenseDate >= start
                && e.ExpenseDate < end)
            .Select(e => new
            {
                e.Quantity,
                e.QuantityUnit,
                Country = e.PurchaseJurisdiction != null ? e.PurchaseJurisdiction.CountryCode : null,
                Region = e.PurchaseJurisdiction != null ? e.PurchaseJurisdiction.Region : null
            })
            .ToListAsync(ct);

        var gallonsByJurisdiction = fuelPurchases
            .Where(p => p.Country is not null && p.Region is not null)
            .GroupBy(p => (p.Country!, p.Region!))
            .ToDictionary(g => g.Key, g => g.Sum(p => ToGallons(p.Quantity!.Value, p.QuantityUnit)));

        var totalMiles = mileage.Sum(m => m.Miles);
        var totalGallons = fuelPurchases.Sum(p => ToGallons(p.Quantity!.Value, p.QuantityUnit));
        var averageMpg = totalGallons > 0 ? Math.Round(totalMiles / totalGallons, 2) : 0m;

        var rates = (await masterUow.Repository<IftaTaxRate>()
                .GetListAsync(r => r.Year == year && r.Quarter == quarter, ct))
            .GroupBy(r => (r.Jurisdiction.CountryCode, r.Jurisdiction.Region ?? string.Empty))
            .ToDictionary(g => g.Key, g => g.First());

        var jurisdictionKeys = mileage
            .Select(m => (m.CountryCode, Region: m.Region ?? string.Empty))
            .Union(gallonsByJurisdiction.Keys.Select(k => (k.Item1, k.Item2)))
            .Distinct()
            .OrderBy(k => k.Item1).ThenBy(k => k.Item2)
            .ToList();

        var rows = new List<IftaJurisdictionRowDto>();
        foreach (var (country, region) in jurisdictionKeys)
        {
            var miles = mileage
                .Where(m => m.CountryCode == country && (m.Region ?? string.Empty) == region)
                .Sum(m => m.Miles);
            var purchased = gallonsByJurisdiction.GetValueOrDefault((country, region));
            var taxable = averageMpg > 0 ? Math.Round(miles / averageMpg, 2) : 0m;
            var netTaxable = Math.Round(taxable - purchased, 2);

            rates.TryGetValue((country, region), out var rate);

            // Surcharge jurisdictions (IN/KY/VA) levy the surcharge on ALL taxable gallons;
            // unlike the base rate it is never offset by purchased gallons.
            decimal? taxDue = rate is null
                ? null
                : Math.Round(netTaxable * rate.RatePerGallon + taxable * (rate.SurchargeRatePerGallon ?? 0m), 2);

            rows.Add(new IftaJurisdictionRowDto
            {
                CountryCode = country,
                Region = region,
                Miles = Math.Round(miles, 2),
                PurchasedGallons = Math.Round(purchased, 2),
                TaxableGallons = taxable,
                NetTaxableGallons = netTaxable,
                RatePerGallon = rate?.RatePerGallon,
                SurchargeRatePerGallon = rate?.SurchargeRatePerGallon,
                TaxDue = taxDue,
                RateMissing = rate is null
            });
        }

        return new IftaReportDto
        {
            Year = year,
            Quarter = quarter,
            IsClosed = false,
            TotalMiles = Math.Round(totalMiles, 2),
            TotalGallons = Math.Round(totalGallons, 2),
            AverageMpg = averageMpg,
            TotalTaxDue = Math.Round(rows.Where(r => r.TaxDue.HasValue).Sum(r => r.TaxDue!.Value), 2),
            HasMissingRates = rows.Any(r => r.RateMissing),
            Jurisdictions = rows
        };
    }

    private static decimal ToGallons(decimal quantity, VolumeUnit? unit) =>
        unit == VolumeUnit.Liters ? quantity / LitersPerGallon : quantity;
}
