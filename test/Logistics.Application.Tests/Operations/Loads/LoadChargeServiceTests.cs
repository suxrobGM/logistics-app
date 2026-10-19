using System.Linq.Expressions;
using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class LoadChargeServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<LoadInvoice, Guid> _invoiceRepo = Substitute.For<ITenantRepository<LoadInvoice, Guid>>();
    private readonly ITenantRepository<LoadAccessorial, Guid> _accessorialRepo =
        Substitute.For<ITenantRepository<LoadAccessorial, Guid>>();
    private readonly IInvoiceTaxApplier _taxApplier = Substitute.For<IInvoiceTaxApplier>();
    private readonly Load _load = LoadInvoiceChargesTests.NewLoad();
    private readonly Trip _trip;
    private readonly LoadInvoice _invoice;
    private readonly LoadChargeService _sut;

    public LoadChargeServiceTests()
    {
        var tenant = TestTenant.Create();
        tenant.Settings.DetentionFreeMinutes = 120;
        tenant.Settings.DetentionRatePerHour = 60m;

        var truck = new Truck { Number = "T-1", Type = TruckType.FreightTruck };
        _trip = Trip.Create("Trip", truck, [_load]);
        _invoice = LoadFactory.CreateInvoice(_load);

        _tenantUow.GetCurrentTenant().Returns(tenant);
        _tenantUow.Repository<LoadInvoice>().Returns(_invoiceRepo);
        _tenantUow.Repository<LoadAccessorial>().Returns(_accessorialRepo);
        _tenantUow.Repository<InvoiceLineItem>().Returns(Substitute.For<ITenantRepository<InvoiceLineItem, Guid>>());
        _tenantUow.Repository<Invoice>().Returns(Substitute.For<ITenantRepository<Invoice, Guid>>());
        _invoiceRepo.GetAsync(Arg.Any<Expression<Func<LoadInvoice, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_invoice);
        _taxApplier.When(t => t.ApplyAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>()))
            .Do(ci => ci.Arg<Invoice>().RecalculateTotals());

        _sut = new LoadChargeService(_tenantUow, _taxApplier);
    }

    private TripStop Stop(int index, int dwellMinutes, bool departed = true)
    {
        var stop = _trip.Stops.OrderBy(s => s.Order).ElementAt(index);
        stop.ArrivedAt = DateTime.UtcNow.AddMinutes(-dwellMinutes - 1);
        stop.DepartedAt = departed ? stop.ArrivedAt.Value.AddMinutes(dwellMinutes) : null;
        return stop;
    }

    [Fact]
    public async Task ApplyDetention_MultiStop_WritesOneDetentionLinePerStop()
    {
        await _sut.ApplyDetentionAsync(Stop(0, 180));
        await _sut.ApplyDetentionAsync(Stop(1, 150));

        var lines = _invoice.LineItems.Where(l => l.Type == InvoiceLineItemType.Detention).ToList();
        Assert.Equal(2, lines.Count);
        Assert.Contains(lines, l => l.Total == 60m);   // 1 h past 2 h free
        Assert.Contains(lines, l => l.Total == 30m);   // 0.5 h
        Assert.Equal(2000m + 90m, _invoice.Total.Amount);
        await _accessorialRepo.Received(2).AddAsync(Arg.Any<LoadAccessorial>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyDetention_SameStopAgain_UpdatesInsteadOfDuplicating()
    {
        var stop = Stop(0, 180);
        await _sut.ApplyDetentionAsync(stop);
        stop.DepartedAt = stop.ArrivedAt!.Value.AddMinutes(240);

        await _sut.ApplyDetentionAsync(stop);

        var detention = Assert.Single(_load.Accessorials);
        Assert.Equal(2m, detention.Quantity);
        Assert.Equal(120m, _invoice.LineItems.Single(l => l.Type == InvoiceLineItemType.Detention).Total);
    }

    [Fact]
    public async Task ApplyDetention_WithinFreeTime_AddsNothing()
    {
        await _sut.ApplyDetentionAsync(Stop(0, 120));

        Assert.Empty(_load.Accessorials);
        Assert.Single(_invoice.LineItems);
    }

    [Fact]
    public async Task ApplyDetention_MissingDeparture_AddsNothing()
    {
        await _sut.ApplyDetentionAsync(Stop(0, 300, departed: false));

        Assert.Empty(_load.Accessorials);
    }

    [Fact]
    public async Task SyncInvoice_PaidInvoice_IsLeftAlone()
    {
        _invoice.Status = InvoiceStatus.Paid;
        _load.FuelSurcharge = 100m;

        var synced = await _sut.SyncInvoiceAsync(_load);

        Assert.False(synced);
        Assert.Single(_invoice.LineItems);
    }

    [Fact]
    public async Task SyncInvoice_LoadCustomerChanged_BillsTheNewCustomer()
    {
        var customer = new Customer { Name = "New Broker" };
        _load.CustomerId = customer.Id;
        _load.Customer = customer;

        var synced = await _sut.SyncInvoiceAsync(_load);

        Assert.True(synced);
        Assert.Equal(customer.Id, _invoice.CustomerId);
        Assert.Same(customer, _invoice.Customer);
        await _taxApplier.Received(1).ApplyAsync(_invoice, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAccessorials_KeepsListedUpdatesAndRemovesTheRest()
    {
        var keep = new LoadAccessorial
        {
            Load = _load, LoadId = _load.Id, Name = "Lumper", LineItemType = InvoiceLineItemType.Lumper,
            Unit = AccessorialUnit.Flat, Rate = 100m
        };
        var drop = new LoadAccessorial
        {
            Load = _load, LoadId = _load.Id, Name = "Layover", LineItemType = InvoiceLineItemType.Layover,
            Unit = AccessorialUnit.Flat, Rate = 250m
        };
        _load.Accessorials.AddRange([keep, drop]);

        await _sut.ReplaceAccessorialsAsync(_load,
        [
            new LoadAccessorialDto { Id = keep.Id, Name = "Lumper", LineItemType = InvoiceLineItemType.Lumper, Rate = 120m },
            new LoadAccessorialDto { Name = "Liftgate", LineItemType = InvoiceLineItemType.Accessorial, Rate = 75m }
        ]);

        Assert.Equal(["Liftgate", "Lumper"], _load.Accessorials.Select(a => a.Name).Order());
        Assert.Equal(120m, keep.Rate);
        _accessorialRepo.Received(1).Delete(drop);
    }
}
