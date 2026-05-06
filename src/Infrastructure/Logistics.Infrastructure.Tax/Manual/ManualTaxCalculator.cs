using Logistics.Application.Services.Tax;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Tax.Data;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Tax.Manual;

/// <summary>
/// Tenant-managed-rate calculator. Authority order:
///   1) Active <c>TenantTaxRate</c> row matching customer jurisdiction (master DB)
///   2) EU VAT default for buyer country (when seller is EU member)
///   3) US state base rate (when seller is US — no county/city layer)
///   4) Other-country VAT/GST default (UK/AU/CA/NZ/JP/...)
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
            return BuildZeroResult(request, TaxBehavior.Exclusive,
                description: "Customer is VAT/tax exempt");
        }

        var sellerCountry = request.TenantTaxResidencyCountry ?? request.TenantAddress.Country;
        var buyerCountry = request.CustomerAddress.Country;

        // Reverse charge for EU cross-border B2B.
        if (request.TenantRegion == Region.Eu &&
            EuVatRules.IsReverseCharge(sellerCountry, buyerCountry, request.CustomerTaxId))
        {
            return BuildZeroResult(request, TaxBehavior.ReverseCharge,
                description: $"Reverse charge — VAT to be accounted by recipient ({buyerCountry})");
        }

        var (rate, code, description, warning) = await ResolveRateAsync(
            request.TenantId, request.CustomerAddress, request.TenantRegion, ct);

        if (rate <= 0m)
        {
            return BuildZeroResult(request, TaxBehavior.Exclusive,
                description: warning ?? "No applicable tax rate found");
        }

        var lines = new List<TaxCalculationLineResult>(request.LineItems.Count);
        decimal totalTax = 0m;
        decimal totalBase = 0m;
        foreach (var li in request.LineItems)
        {
            var taxAmount = decimal.Round(li.NetAmount * rate / 100m, 2);
            totalTax += taxAmount;
            totalBase += li.NetAmount;
            lines.Add(new TaxCalculationLineResult
            {
                LineItemId = li.LineItemId,
                RatePercent = rate,
                TaxAmount = taxAmount,
                TaxCode = li.TaxCode ?? code
            });
        }

        var jurisdiction = new TaxJurisdiction
        {
            CountryCode = buyerCountry,
            Region = string.IsNullOrEmpty(request.CustomerAddress.State) ? null : request.CustomerAddress.State
        };

        var breakdown = new[]
        {
            new InvoiceTaxLine
            {
                RatePercent = rate,
                BaseAmount = decimal.Round(totalBase, 2),
                TaxAmount = decimal.Round(totalTax, 2),
                Jurisdiction = jurisdiction,
                TaxCode = code,
                Description = description
            }
        };

        return new TaxCalculationResult
        {
            TaxBehavior = TaxBehavior.Exclusive,
            Lines = lines,
            Breakdown = breakdown,
            Warning = warning
        };
    }

    private async Task<(decimal Rate, string? Code, string Description, string? Warning)> ResolveRateAsync(
        Guid tenantId, Address customerAddress, Region tenantRegion, CancellationToken ct)
    {
        // 1. Tenant-managed rate.
        var tenantRate = await FindTenantRateAsync(tenantId, customerAddress, ct);
        if (tenantRate is not null)
        {
            return (tenantRate.RatePercent, tenantRate.TaxCode,
                tenantRate.Description ?? $"Tenant rate ({tenantRate.Jurisdiction})", null);
        }

        // 2. EU default.
        if (tenantRegion == Region.Eu && EuVatRules.IsEuMember(customerAddress.Country))
        {
            var euRate = EuVatRates.GetStandardRate(customerAddress.Country);
            if (euRate is { } rate)
            {
                return (rate, null, $"VAT {rate:0.##}% — {customerAddress.Country} (default)", null);
            }
        }

        // 3. US state default.
        if (tenantRegion == Region.Us && string.Equals(customerAddress.Country, "US", StringComparison.OrdinalIgnoreCase))
        {
            var stateRate = UsSalesTaxRates.GetStateBaseRate(customerAddress.State);
            if (stateRate is { } rate)
            {
                return (rate, null,
                    $"State sales tax {rate:0.##}% — {customerAddress.State}",
                    "State sales tax only — local taxes not included; use Stripe Tax for full destination-based calculation");
            }
        }

        // 4. Other-country default.
        var otherRate = OtherCountryRates.GetRate(customerAddress.Country);
        if (otherRate is { } o)
        {
            return (o, null, $"Tax {o:0.##}% — {customerAddress.Country} (default)", null);
        }

        logger.LogWarning("No manual tax rate resolved for tenant {TenantId} country={Country} state={State}",
            tenantId, customerAddress.Country, customerAddress.State);
        return (0m, null, "No applicable tax rate", "No tax rate configured for this jurisdiction");
    }

    private async Task<TenantTaxRate?> FindTenantRateAsync(
        Guid tenantId, Address customerAddress, CancellationToken ct)
    {
        var rates = await masterUow.Repository<TenantTaxRate>()
            .GetListAsync(r => r.TenantId == tenantId, ct);
        var now = DateTime.UtcNow;
        return rates
            .Where(r => r.IsActiveOn(now))
            .Where(r => r.Jurisdiction.CountryCode.Equals(customerAddress.Country, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r =>
                !string.IsNullOrEmpty(r.Jurisdiction.Region) &&
                r.Jurisdiction.Region.Equals(customerAddress.State, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(r => r.EffectiveFrom)
            .FirstOrDefault(r =>
                string.IsNullOrEmpty(r.Jurisdiction.Region) ||
                r.Jurisdiction.Region.Equals(customerAddress.State, StringComparison.OrdinalIgnoreCase));
    }

    private static TaxCalculationResult BuildZeroResult(TaxCalculationRequest request, TaxBehavior behavior, string description)
    {
        var lines = request.LineItems.Select(li => new TaxCalculationLineResult
        {
            LineItemId = li.LineItemId,
            RatePercent = 0m,
            TaxAmount = 0m,
            TaxCode = li.TaxCode
        }).ToList();

        var totalBase = decimal.Round(request.LineItems.Sum(l => l.NetAmount), 2);
        var jurisdiction = new TaxJurisdiction { CountryCode = request.CustomerAddress.Country };
        var breakdown = new[]
        {
            new InvoiceTaxLine
            {
                RatePercent = 0m,
                BaseAmount = totalBase,
                TaxAmount = 0m,
                Jurisdiction = jurisdiction,
                Description = description
            }
        };

        return new TaxCalculationResult
        {
            TaxBehavior = behavior,
            Lines = lines,
            Breakdown = breakdown
        };
    }
}
