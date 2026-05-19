using System.Globalization;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Payments.Stripe;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Tax;
using Logistics.Application.Abstractions.Tax;
using DomainAddress = Logistics.Domain.Primitives.ValueObjects.Address;
using DomainTaxJurisdiction = Logistics.Domain.Primitives.ValueObjects.TaxJurisdiction;

namespace Logistics.Infrastructure.Tax.Stripe;

/// <summary>
/// Region-agnostic tax calculator backed by <see cref="IStripeTaxCalculationApi"/>.
/// One API call covers EU VAT, US destination-based sales tax, UK VAT, AU/NZ GST,
/// CA GST/PST/HST. Reverse charge and "not collecting" warnings are detected from the response.
/// </summary>
internal sealed class StripeTaxCalculator(
    IStripeTaxCalculationApi api,
    IStripeTaxConfigService configService,
    ILogger<StripeTaxCalculator> logger) : ITaxCalculator
{
    private const string NotCollectingWarning =
        "Stripe Tax: tenant not registered in customer jurisdiction; tax not collected.";

    public async Task<TaxCalculationResult> CalculateAsync(TaxCalculationRequest request, CancellationToken ct = default)
    {
        if (request.IsCustomerVatExempt || request.LineItems.Count == 0)
        {
            return TaxCalculationResult.Empty();
        }

        var defaultCode = await configService.GetDefaultTaxCodeAsync(ct);
        var options = BuildOptions(request, defaultCode);

        try
        {
            var calc = await api.CreateAsync(options, ct);
            return MapResult(request, calc);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex,
                "Stripe Tax calculation failed for tenant {TenantId}. Code={Code}",
                request.TenantId, ex.StripeError?.Code);

            return TaxCalculationResult.Empty() with
            {
                Warning = $"Stripe Tax error: {ex.StripeError?.Code ?? ex.Message}"
            };
        }
    }

    private static CalculationCreateOptions BuildOptions(TaxCalculationRequest request, string defaultCode) => new()
    {
        Currency = request.Currency.ToLowerInvariant(),
        LineItems = request.LineItems
            .Select(li => new CalculationLineItemOptions
            {
                Amount = StripeMoneyConversion.ToMinorUnits(li.NetAmount, request.Currency),
                Reference = li.LineItemId.ToString(),
                TaxCode = li.TaxCode ?? defaultCode
            })
            .ToList(),
        CustomerDetails = new CalculationCustomerDetailsOptions
        {
            Address = ToStripeAddress(request.CustomerAddress),
            AddressSource = "shipping",
            TaxIds = string.IsNullOrWhiteSpace(request.CustomerTaxId)
                ? null
                :
                [
                    new CalculationCustomerDetailsTaxIdOptions
                    {
                        Type = StripeTaxIdTypes.Infer(request.CustomerAddress.Country, request.CustomerTaxId),
                        Value = request.CustomerTaxId
                    }
                ]
        },
        Expand = ["line_items.data.tax_breakdown"]
    };

    private static AddressOptions ToStripeAddress(DomainAddress a) => new()
    {
        Line1 = a.Line1,
        Line2 = a.Line2,
        City = a.City,
        State = a.State,
        PostalCode = a.ZipCode,
        Country = a.Country
    };

    private static TaxCalculationResult MapResult(TaxCalculationRequest request, Calculation calc)
    {
        var (lineResults, behavior, warning) = MapLines(request, calc);
        var breakdown = MapBreakdown(calc, request);

        return new TaxCalculationResult
        {
            TaxBehavior = behavior,
            Lines = lineResults,
            Breakdown = breakdown,
            Warning = warning
        };
    }

    private static (List<TaxCalculationLineResult> Lines, TaxBehavior Behavior, string? Warning) MapLines(
        TaxCalculationRequest request, Calculation calc)
    {
        var currency = request.Currency;
        var perLine = new Dictionary<Guid, TaxCalculationLineResult>();
        var behavior = TaxBehavior.Exclusive;
        string? warning = null;

        foreach (var item in calc.LineItems?.Data ?? [])
        {
            if (!Guid.TryParse(item.Reference, out var lineId)) continue;

            (var rate, var lineBehavior, warning) = InspectBreakdown(item.TaxBreakdown, warning);
            if (lineBehavior == TaxBehavior.ReverseCharge) behavior = TaxBehavior.ReverseCharge;

            perLine[lineId] = new TaxCalculationLineResult
            {
                LineItemId = lineId,
                RatePercent = rate,
                TaxAmount = decimal.Round(StripeMoneyConversion.FromMinorUnits(item.AmountTax, currency), 2),
                TaxCode = item.TaxCode
            };
        }

        // Defensively zero-fill any line Stripe didn't echo back.
        foreach (var li in request.LineItems)
        {
            perLine.TryAdd(li.LineItemId, new TaxCalculationLineResult
            {
                LineItemId = li.LineItemId,
                RatePercent = 0m,
                TaxAmount = 0m,
                TaxCode = li.TaxCode
            });
        }

        return (perLine.Values.ToList(), behavior, warning);
    }

    private static (decimal Rate, TaxBehavior Behavior, string? Warning) InspectBreakdown(
        IList<CalculationLineItemTaxBreakdown>? breakdown, string? existingWarning)
    {
        if (breakdown is null || breakdown.Count == 0)
        {
            return (0m, TaxBehavior.Exclusive, existingWarning);
        }

        decimal rate = 0m;
        var behavior = TaxBehavior.Exclusive;
        var warning = existingWarning;

        foreach (var b in breakdown)
        {
            if (rate == 0m && TryParseRate(b.TaxRateDetails?.PercentageDecimal, out var parsed))
            {
                rate = parsed;
            }

            if (Equals(b.TaxabilityReason, "reverse_charge"))
            {
                behavior = TaxBehavior.ReverseCharge;
            }
            else if (Equals(b.TaxabilityReason, "not_collecting"))
            {
                warning ??= NotCollectingWarning;
            }
        }

        return (rate, behavior, warning);
    }

    private static List<InvoiceTaxLine> MapBreakdown(Calculation calc, TaxCalculationRequest request)
    {
        var lines = (calc.TaxBreakdown ?? [])
            .Select(b => MapBreakdownLine(b, request.Currency))
            .ToList();

        // Reverse-charge responses can have an empty top-level breakdown; surface a synthetic
        // explanatory line so the PDF/UI can still render the legally-required notice.
        if (lines.Count == 0 && calc.LineItems?.Data?.Any(IsReverseCharge) == true)
        {
            lines.Add(new InvoiceTaxLine
            {
                RatePercent = 0m,
                BaseAmount = decimal.Round(request.LineItems.Sum(l => l.NetAmount), 2),
                TaxAmount = 0m,
                Jurisdiction = new DomainTaxJurisdiction { CountryCode = request.CustomerAddress.Country },
                Description = "Reverse charge — VAT to be accounted by recipient"
            });
        }

        return lines;
    }

    private static InvoiceTaxLine MapBreakdownLine(CalculationTaxBreakdown b, string currency)
    {
        var details = b.TaxRateDetails;
        var rate = TryParseRate(details?.PercentageDecimal, out var parsed) ? parsed : 0m;
        var country = details?.Country ?? string.Empty;
        var state = details?.State;
        var taxType = details?.TaxType ?? "tax";

        var description = string.IsNullOrEmpty(state)
            ? $"{taxType.ToUpperInvariant()} {rate:0.##}% — {country}"
            : $"{taxType.ToUpperInvariant()} {rate:0.##}% — {country}-{state}";

        return new InvoiceTaxLine
        {
            RatePercent = rate,
            BaseAmount = decimal.Round(StripeMoneyConversion.FromMinorUnits(b.TaxableAmount, currency), 2),
            TaxAmount = decimal.Round(StripeMoneyConversion.FromMinorUnits(b.Amount, currency), 2),
            Jurisdiction = new DomainTaxJurisdiction { CountryCode = country, Region = state },
            Description = description
        };
    }

    private static bool IsReverseCharge(CalculationLineItem item) =>
        item.TaxBreakdown?.Any(b => Equals(b.TaxabilityReason, "reverse_charge")) == true;

    private static bool Equals(string? a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static bool TryParseRate(string? percent, out decimal rate)
    {
        if (percent is not null && decimal.TryParse(percent, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
        {
            rate = v;
            return true;
        }

        rate = 0m;
        return false;
    }
}
