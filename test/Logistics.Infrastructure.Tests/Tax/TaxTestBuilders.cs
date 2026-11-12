using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Logistics.Application.Abstractions.Tax;

namespace Logistics.Infrastructure.Tax.Tests;

internal static class TaxTestBuilders
{
    public static Address Address(string country = "US", string state = "CA", string city = "San Francisco")
    {
        return new()
        {
            Line1 = "1 Market St",
            City = city,
            ZipCode = "94105",
            State = state,
            Country = country
        };
    }

    public static TaxCalculationRequest Request(
        string country = "US",
        string state = "CA",
        Region tenantRegion = Region.US,
        string? customerTaxId = null,
        bool exempt = false,
        decimal[]? lineAmounts = null,
        string? tenantCountry = null,
        string currency = "USD",
        Guid? tenantId = null)
    {
        return new()
        {
            Currency = currency,
            TenantId = tenantId ?? Guid.NewGuid(),
            TenantRegion = tenantRegion,
            TenantAddress = Address(country: tenantCountry ?? (tenantRegion == Region.EU ? "DE" : "US"),
                                state: tenantRegion == Region.EU ? "" : "TX"),
            TenantTaxId = tenantRegion == Region.EU ? "DE123456789" : null,
            TenantTaxResidencyCountry = tenantCountry,
            CustomerAddress = Address(country, state),
            CustomerTaxId = customerTaxId,
            IsCustomerVatExempt = exempt,
            LineItems = (lineAmounts ?? [100m]).Select(a => new TaxCalculationLineItem
            {
                LineItemId = Guid.NewGuid(),
                NetAmount = a,
                Description = "Test"
            }).ToList()
        };
    }

    public static IMasterUnitOfWork MasterUowWithRates(params TenantTaxRate[] rates)
    {
        var repo = Substitute.For<IMasterRepository<TenantTaxRate, Guid>>();
        repo.GetListAsync(
                Arg.Any<Expression<Func<TenantTaxRate, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var predicate = ((Expression<Func<TenantTaxRate, bool>>)call[0]).Compile();
                return Task.FromResult(rates.Where(predicate).ToList());
            });

        var uow = Substitute.For<IMasterUnitOfWork>();
        uow.Repository<TenantTaxRate>().Returns(repo);
        return uow;
    }

    public static TenantTaxRate TenantRate(
        Guid tenantId,
        string country,
        string? region = null,
        decimal ratePercent = 17.5m,
        DateTime? from = null,
        DateTime? to = null,
        string? taxCode = null,
        string? description = null)
    {
        return new()
        {
            TenantId = tenantId,
            Jurisdiction = new TaxJurisdiction { CountryCode = country, Region = region },
            RatePercent = ratePercent,
            EffectiveFrom = from ?? DateTime.UtcNow.AddDays(-1),
            EffectiveTo = to,
            TaxCode = taxCode,
            Description = description
        };
    }
}
