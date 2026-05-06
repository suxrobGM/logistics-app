using Logistics.Application.Services.Tax;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Payments.Stripe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Tax;

namespace Logistics.Infrastructure.Tax.Stripe;

/// <summary>
/// Region-agnostic tax calculator backed by <c>Stripe.Tax.CalculationService</c>.
/// One API call handles EU VAT, US destination-based sales tax (with state/county/city layering),
/// UK VAT, AU GST, CA GST/PST/HST. Reverse charge and "not collecting" warnings are detected
/// from the response shape.
/// </summary>
internal sealed class StripeTaxCalculator : ITaxCalculator
{
    private readonly ILogger<StripeTaxCalculator> logger;
    private readonly IStripeTaxConfigService configService;

    public StripeTaxCalculator(
        IOptions<StripeOptions> stripeOptions,
        IStripeTaxConfigService configService,
        ILogger<StripeTaxCalculator> logger)
    {
        this.logger = logger;
        this.configService = configService;
        if (string.IsNullOrEmpty(StripeConfiguration.ApiKey))
        {
            StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;
        }
    }

    public async Task<TaxCalculationResult> CalculateAsync(TaxCalculationRequest request, CancellationToken ct = default)
    {
        if (request.IsCustomerVatExempt || request.LineItems.Count == 0)
        {
            return TaxCalculationResult.Empty();
        }

        var defaultCode = await configService.GetDefaultTaxCodeAsync(ct);
        var amounts = ToStripeAmounts(request, defaultCode);
        var options = new CalculationCreateOptions
        {
            Currency = request.Currency.ToLowerInvariant(),
            LineItems = amounts,
            CustomerDetails = new CalculationCustomerDetailsOptions
            {
                Address = ToAddressOptions(request.CustomerAddress),
                AddressSource = "shipping",
                TaxIds = string.IsNullOrWhiteSpace(request.CustomerTaxId)
                    ? null
                    :
                    [
                        new CalculationCustomerDetailsTaxIdOptions
                        {
                            Type = InferTaxIdType(request.CustomerAddress.Country, request.CustomerTaxId),
                            Value = request.CustomerTaxId
                        }
                    ]
            },
            Expand = ["line_items.data.tax_breakdown"]
        };

        try
        {
            var calc = await new CalculationService().CreateAsync(options, cancellationToken: ct);
            return MapResult(request, calc);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex,
                "Stripe Tax calculation failed for tenant {TenantId}; returning zero result. Code={Code}",
                request.TenantId, ex.StripeError?.Code);

            return TaxCalculationResult.Empty() with
            {
                Warning = $"Stripe Tax error: {ex.StripeError?.Code ?? ex.Message}"
            };
        }
    }

    private static List<CalculationLineItemOptions> ToStripeAmounts(TaxCalculationRequest request, string defaultCode)
    {
        var list = new List<CalculationLineItemOptions>(request.LineItems.Count);
        foreach (var li in request.LineItems)
        {
            list.Add(new CalculationLineItemOptions
            {
                Amount = ToMinorUnits(li.NetAmount, request.Currency),
                Reference = li.LineItemId.ToString(),
                TaxCode = li.TaxCode ?? defaultCode
            });
        }
        return list;
    }

    private static AddressOptions ToAddressOptions(Logistics.Domain.Primitives.ValueObjects.Address a) => new()
    {
        Line1 = a.Line1,
        Line2 = a.Line2,
        City = a.City,
        State = a.State,
        PostalCode = a.ZipCode,
        Country = a.Country
    };

    private static long ToMinorUnits(decimal amount, string currency)
    {
        // Most ISO-4217 currencies use 2 fractional digits. JPY/KRW are zero-decimal.
        var zeroDecimal = currency.Equals("JPY", StringComparison.OrdinalIgnoreCase)
                       || currency.Equals("KRW", StringComparison.OrdinalIgnoreCase);
        return zeroDecimal
            ? (long)decimal.Round(amount, 0, MidpointRounding.AwayFromZero)
            : (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
    }

    private static decimal FromMinorUnits(long minor, string currency)
    {
        var zeroDecimal = currency.Equals("JPY", StringComparison.OrdinalIgnoreCase)
                       || currency.Equals("KRW", StringComparison.OrdinalIgnoreCase);
        return zeroDecimal ? minor : minor / 100m;
    }

    private static string InferTaxIdType(string country, string taxId)
    {
        // Stripe uses a per-country tax ID type code (eu_vat, us_ein, gb_vat, etc.).
        // Best-effort mapping; full table at https://stripe.com/docs/billing/customer/tax-ids
        return country.ToUpperInvariant() switch
        {
            "GB" => "gb_vat",
            "US" => "us_ein",
            "AU" => "au_abn",
            "CA" => "ca_gst_hst",
            "JP" => "jp_cn",
            "IN" => "in_gst",
            "MX" => "mx_rfc",
            "BR" => "br_cnpj",
            "ZA" => "za_vat",
            _ when taxId.StartsWith("CH") => "ch_vat",
            _ when taxId.StartsWith("NO") => "no_vat",
            _ => "eu_vat"
        };
    }

    private TaxCalculationResult MapResult(TaxCalculationRequest request, Calculation calc)
    {
        var currency = request.Currency;
        TaxBehavior behavior = TaxBehavior.Exclusive;
        string? warning = null;
        var perLine = new Dictionary<Guid, TaxCalculationLineResult>();

        if (calc.LineItems?.Data is { Count: > 0 } items)
        {
            foreach (var item in items)
            {
                if (!Guid.TryParse(item.Reference, out var lineId)) continue;

                var taxAmount = FromMinorUnits(item.AmountTax, currency);
                decimal? rate = null;
                string? taxCode = item.TaxCode;

                if (item.TaxBreakdown is { Count: > 0 })
                {
                    var firstWithRate = item.TaxBreakdown.FirstOrDefault(b =>
                        b.TaxRateDetails?.PercentageDecimal is not null);
                    if (firstWithRate is not null && decimal.TryParse(
                            firstWithRate.TaxRateDetails.PercentageDecimal,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var pct))
                    {
                        rate = pct;
                    }

                    foreach (var b in item.TaxBreakdown)
                    {
                        var reason = b.TaxabilityReason;
                        if (string.Equals(reason, "reverse_charge", StringComparison.OrdinalIgnoreCase))
                        {
                            behavior = TaxBehavior.ReverseCharge;
                        }
                        if (string.Equals(reason, "not_collecting", StringComparison.OrdinalIgnoreCase))
                        {
                            warning ??= "Stripe Tax: tenant not registered in customer jurisdiction; tax not collected.";
                        }
                    }
                }

                perLine[lineId] = new TaxCalculationLineResult
                {
                    LineItemId = lineId,
                    RatePercent = rate ?? 0m,
                    TaxAmount = decimal.Round(taxAmount, 2),
                    TaxCode = taxCode
                };
            }
        }

        // Fill in zeros for any line Stripe didn't report (defensive).
        foreach (var li in request.LineItems)
        {
            if (!perLine.ContainsKey(li.LineItemId))
            {
                perLine[li.LineItemId] = new TaxCalculationLineResult
                {
                    LineItemId = li.LineItemId,
                    RatePercent = 0m,
                    TaxAmount = 0m,
                    TaxCode = li.TaxCode
                };
            }
        }

        var breakdown = BuildBreakdown(calc, currency);
        if (breakdown.Count == 0 && behavior == TaxBehavior.ReverseCharge)
        {
            breakdown =
            [
                new InvoiceTaxLine
                {
                    RatePercent = 0m,
                    BaseAmount = decimal.Round(request.LineItems.Sum(l => l.NetAmount), 2),
                    TaxAmount = 0m,
                    Jurisdiction = new TaxJurisdiction { CountryCode = request.CustomerAddress.Country },
                    Description = "Reverse charge — VAT to be accounted by recipient"
                }
            ];
        }

        return new TaxCalculationResult
        {
            TaxBehavior = behavior,
            Lines = perLine.Values.ToList(),
            Breakdown = breakdown,
            Warning = warning
        };
    }

    private static List<InvoiceTaxLine> BuildBreakdown(Calculation calc, string currency)
    {
        if (calc.TaxBreakdown is null) return [];

        var output = new List<InvoiceTaxLine>(calc.TaxBreakdown.Count);
        foreach (var b in calc.TaxBreakdown)
        {
            var details = b.TaxRateDetails;
            decimal rate = 0m;
            if (details?.PercentageDecimal is { } pctText &&
                decimal.TryParse(pctText, System.Globalization.CultureInfo.InvariantCulture, out var pct))
            {
                rate = pct;
            }

            var country = details?.Country ?? string.Empty;
            var state = details?.State;
            var taxType = details?.TaxType ?? "tax";

            var description = string.IsNullOrEmpty(state)
                ? $"{taxType.ToUpperInvariant()} {rate:0.##}% — {country}"
                : $"{taxType.ToUpperInvariant()} {rate:0.##}% — {country}-{state}";

            output.Add(new InvoiceTaxLine
            {
                RatePercent = rate,
                BaseAmount = decimal.Round(FromMinorUnits(b.TaxableAmount, currency), 2),
                TaxAmount = decimal.Round(FromMinorUnits(b.Amount, currency), 2),
                Jurisdiction = new TaxJurisdiction { CountryCode = country, Region = state },
                TaxCode = null,
                Description = description
            });
        }
        return output;
    }
}
