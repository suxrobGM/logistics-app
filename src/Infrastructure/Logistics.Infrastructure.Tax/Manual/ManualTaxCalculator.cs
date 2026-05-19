using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Tax.Data;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Tax;

namespace Logistics.Infrastructure.Tax.Manual;

/// <summary>
/// Tenant-managed-rate calculator. Authority order:
///   1) Active <c>TenantTaxRate</c> row matching customer jurisdiction (master DB)
///   2) EU VAT default for buyer country (when seller is EU member)
///   3) US state base rate (when seller is US — no county/city layer)
///   4) Other-country VAT/GST default (UK/AU/CA/NZ/JP/IN/MX/BR/ZA)
///   5) Zero rate as last resort
///
/// Reverse charge: applied only for EU-to-EU B2B with a customer VAT ID.
/// </summary>
internal sealed class ManualTaxCalculator(
    IMasterUnitOfWork masterUow,
    ILogger<ManualTaxCalculator> logger) : ITaxCalculator
{
    public async Task<TaxCalculationResult> CalculateAsync(TaxCalculationRequest request, CancellationToken ct = default)
    {
        if (request.IsCustomerVatExempt)
        {
            return ZeroResult(request, TaxBehavior.Exclusive, "Customer is VAT/tax exempt");
        }

        if (IsEuReverseCharge(request))
        {
            return ZeroResult(request, TaxBehavior.ReverseCharge,
                $"Reverse charge — VAT to be accounted by recipient ({request.CustomerAddress.Country})");
        }

        var resolution = await ResolveRateAsync(request, ct);
        return resolution.Rate <= 0m
            ? ZeroResult(request, TaxBehavior.Exclusive, resolution.Description, resolution.Warning)
            : ApplyRate(request, resolution);
    }

    private static bool IsEuReverseCharge(TaxCalculationRequest request)
    {
        if (request.TenantRegion != Region.EU)
        {
            return false;
        }
        var sellerCountry = request.TenantTaxResidencyCountry ?? request.TenantAddress.Country;
        return EuVatRules.IsReverseCharge(sellerCountry, request.CustomerAddress.Country, request.CustomerTaxId);
    }

    private async Task<RateResolution> ResolveRateAsync(TaxCalculationRequest request, CancellationToken ct)
    {
        var tenantRate = await FindTenantRateAsync(request.TenantId, request.CustomerAddress, ct);
        if (tenantRate is not null)
        {
            return new RateResolution(
                tenantRate.RatePercent,
                tenantRate.TaxCode,
                tenantRate.Description ?? $"Tenant rate ({tenantRate.Jurisdiction})");
        }

        if (request.TenantRegion == Region.EU &&
            EuVatRules.IsEuMember(request.CustomerAddress.Country) &&
            EuVatRates.GetStandardRate(request.CustomerAddress.Country) is { } euRate)
        {
            return new RateResolution(euRate, null,
                $"VAT {euRate:0.##}% — {request.CustomerAddress.Country} (default)");
        }

        if (request.TenantRegion == Region.US &&
            string.Equals(request.CustomerAddress.Country, "US", StringComparison.OrdinalIgnoreCase) &&
            UsSalesTaxRates.GetStateBaseRate(request.CustomerAddress.State) is { } usRate)
        {
            return new RateResolution(usRate, null,
                $"State sales tax {usRate:0.##}% — {request.CustomerAddress.State}",
                Warning: "State sales tax only — local taxes not included; use Stripe Tax for full destination-based calculation");
        }

        if (OtherCountryRates.GetRate(request.CustomerAddress.Country) is { } otherRate)
        {
            return new RateResolution(otherRate, null,
                $"Tax {otherRate:0.##}% — {request.CustomerAddress.Country} (default)");
        }

        logger.LogWarning(
            "No manual tax rate resolved for tenant {TenantId} country={Country} state={State}",
            request.TenantId, request.CustomerAddress.Country, request.CustomerAddress.State);

        return new RateResolution(0m, null, "No applicable tax rate",
            Warning: "No tax rate configured for this jurisdiction");
    }

    private async Task<TenantTaxRate?> FindTenantRateAsync(
        Guid tenantId, Address customerAddress, CancellationToken ct)
    {
        var rates = await masterUow.Repository<TenantTaxRate>()
            .GetListAsync(r => r.TenantId == tenantId, ct);
        var now = DateTime.UtcNow;

        return rates
            .Where(r => r.IsActiveOn(now))
            .Where(r => r.Jurisdiction.CountryCode.Equals(
                customerAddress.Country, StringComparison.OrdinalIgnoreCase))
            .Where(r => string.IsNullOrEmpty(r.Jurisdiction.Region) ||
                        r.Jurisdiction.Region.Equals(customerAddress.State, StringComparison.OrdinalIgnoreCase))
            // State-specific rates beat country-only; among ties, pick the most recent.
            .OrderByDescending(r => string.IsNullOrEmpty(r.Jurisdiction.Region) ? 0 : 1)
            .ThenByDescending(r => r.EffectiveFrom)
            .FirstOrDefault();
    }

    private static TaxCalculationResult ApplyRate(TaxCalculationRequest request, RateResolution resolution)
    {
        var lines = new List<TaxCalculationLineResult>(request.LineItems.Count);
        decimal totalTax = 0m;
        decimal totalBase = 0m;

        foreach (var li in request.LineItems)
        {
            var taxAmount = decimal.Round(li.NetAmount * resolution.Rate / 100m, 2);
            totalTax += taxAmount;
            totalBase += li.NetAmount;

            lines.Add(new TaxCalculationLineResult
            {
                LineItemId = li.LineItemId,
                RatePercent = resolution.Rate,
                TaxAmount = taxAmount,
                TaxCode = li.TaxCode ?? resolution.TaxCode
            });
        }

        var breakdown = new[]
        {
            new InvoiceTaxLine
            {
                RatePercent = resolution.Rate,
                BaseAmount = decimal.Round(totalBase, 2),
                TaxAmount = decimal.Round(totalTax, 2),
                Jurisdiction = JurisdictionFor(request.CustomerAddress),
                TaxCode = resolution.TaxCode,
                Description = resolution.Description
            }
        };

        return new TaxCalculationResult
        {
            TaxBehavior = TaxBehavior.Exclusive,
            Lines = lines,
            Breakdown = breakdown,
            Warning = resolution.Warning
        };
    }

    private static TaxCalculationResult ZeroResult(
        TaxCalculationRequest request, TaxBehavior behavior, string description, string? warning = null)
    {
        var lines = request.LineItems
            .Select(li => new TaxCalculationLineResult
            {
                LineItemId = li.LineItemId,
                RatePercent = 0m,
                TaxAmount = 0m,
                TaxCode = li.TaxCode
            })
            .ToList();

        var breakdown = new[]
        {
            new InvoiceTaxLine
            {
                RatePercent = 0m,
                BaseAmount = decimal.Round(request.LineItems.Sum(l => l.NetAmount), 2),
                TaxAmount = 0m,
                Jurisdiction = JurisdictionFor(request.CustomerAddress),
                Description = description
            }
        };

        return new TaxCalculationResult
        {
            TaxBehavior = behavior,
            Lines = lines,
            Breakdown = breakdown,
            Warning = warning
        };
    }

    private static TaxJurisdiction JurisdictionFor(Address customerAddress) => new()
    {
        CountryCode = customerAddress.Country,
        Region = string.IsNullOrEmpty(customerAddress.State) ? null : customerAddress.State
    };

    private sealed record RateResolution(decimal Rate, string? TaxCode, string Description, string? Warning = null);
}
