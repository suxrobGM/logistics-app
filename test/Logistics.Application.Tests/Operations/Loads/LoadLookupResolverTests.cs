using Logistics.Application.Modules.Operations.Loads;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mappings;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

/// <summary>
/// The loads here have every navigation unset, so each value the DTO shows can only have come from
/// the batched lookup - a mapper that went back to the navigations would leave it blank.
/// </summary>
public class LoadLookupResolverTests
{
    private static readonly Address Somewhere =
        new() { Line1 = "1 A St", City = "NYC", State = "NY", ZipCode = "10001", Country = "US" };

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();

    public LoadLookupResolverTests()
    {
        _tenantUow.StubLoadLookups();
    }

    private void Returns<T>(params T[] rows) where T : class, IEntity<Guid>, ITenantEntity
    {
        var repo = Substitute.For<ITenantRepository<T, Guid>>();
        repo.Query().Returns(rows.BuildMock());
        _tenantUow.Repository<T>().Returns(repo);
    }

    [Fact]
    public async Task ResolveAsync_MapsRelatedValuesWithoutTheNavigations()
    {
        var customer = new Customer { Name = "ACME" };
        var dispatcher = new Employee { Email = "d@x.com", FirstName = "Dana", LastName = "Disp" };
        var driver = new Employee { Email = "r@x.com", FirstName = "Rick", LastName = "Road" };
        var truck = new Truck { Number = "T-7", Type = TruckType.FreightTruck, MainDriverId = driver.Id };
        var load = new Load
        {
            Name = "Load 1",
            Type = LoadType.GeneralFreight,
            OriginAddress = Somewhere,
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = Somewhere,
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = new Money { Amount = 1000m, Currency = "USD" },
            Customer = null!,
            CustomerId = customer.Id,
            AssignedTruckId = truck.Id,
            AssignedDispatcherId = dispatcher.Id
        };
        var trip = new Trip { Name = "Trip 9" };
        var invoice = new LoadInvoice
        {
            Subtotal = load.DeliveryCost,
            TaxTotal = Money.Zero("USD"),
            Total = load.DeliveryCost,
            LoadId = load.Id,
            CustomerId = customer.Id
        };
        invoice.LineItems.Add(new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Description = "Linehaul",
            Type = InvoiceLineItemType.BaseRate,
            Amount = load.DeliveryCost
        });

        Returns(customer);
        Returns(dispatcher, driver);
        Returns(truck);
        Returns(new TripStop
        {
            Type = TripStopType.PickUp,
            Trip = trip,
            Load = load,
            LoadId = load.Id,
            Address = Somewhere,
            Location = new GeoPoint(0, 0)
        });
        Returns(invoice);

        var lookup = await LoadLookupResolver.ResolveAsync(_tenantUow, [load]);
        var dto = load.ToDto(lookup);

        Assert.Equal("ACME", dto.Customer?.Name);
        Assert.Equal("Dana Disp", dto.AssignedDispatcherName);
        Assert.Equal("T-7", dto.AssignedTruckNumber);
        Assert.Equal(["Rick Road"], dto.AssignedTruckDriversName!);
        Assert.Equal("Trip 9", dto.TripName);
        Assert.Equal(invoice.Id, dto.Invoice?.Id);
        Assert.Equal(load.Number, dto.Invoice?.LoadNumber);
        Assert.Single(dto.Invoice!.LineItems);
    }
}
