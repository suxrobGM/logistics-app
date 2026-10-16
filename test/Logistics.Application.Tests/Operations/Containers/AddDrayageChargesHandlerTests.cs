using Logistics.Application.Modules.Operations.Containers.Commands;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Tests.Operations.Loads;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Containers;

public class AddDrayageChargesHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc);

    private static Container OverdueContainer() => new()
    {
        Number = "MSCU1234567",
        IsoType = ContainerIsoType.Hc40,
        LastFreeDay = new DateOnly(2026, 10, 9),
        DemurrageRatePerDay = 150m,
        PickedUpAt = new DateTime(2026, 10, 11, 8, 0, 0, DateTimeKind.Utc),
        EmptyReturnDeadline = new DateOnly(2026, 10, 11),
        PerDiemRatePerDay = 100m,
        ChassisNumber = "DCLZ123456",
        ChassisProvider = ChassisProvider.Dcli,
        ChassisRatePerDay = 30m
    };

    [Fact]
    public void BuildAccessorials_AllCharges_PricesRatePerDay()
    {
        var load = LoadInvoiceChargesTests.NewLoad();
        var container = OverdueContainer();

        var accessorials = AddDrayageChargesHandler.BuildAccessorials(load, container, container.GetFreeTime(Now));

        Assert.Collection(accessorials,
            demurrage =>
            {
                Assert.Equal(InvoiceLineItemType.Demurrage, demurrage.LineItemType);
                Assert.Equal(150m, demurrage.Rate);
                Assert.Equal(2, demurrage.Quantity);
            },
            perDiem =>
            {
                Assert.Equal(InvoiceLineItemType.PerDiem, perDiem.LineItemType);
                Assert.Equal(1, perDiem.Quantity);
            },
            chassis =>
            {
                Assert.Equal(InvoiceLineItemType.Chassis, chassis.LineItemType);
                Assert.Equal(2, chassis.Quantity);
                Assert.Contains("DCLZ123456", chassis.Name);
            });
        Assert.All(accessorials, a =>
        {
            Assert.Equal(load.Id, a.LoadId);
            Assert.Equal(AccessorialUnit.PerDay, a.Unit);
        });
    }

    [Fact]
    public void BuildAccessorials_WithinFreeTimeAndOwnChassis_AddsNothing()
    {
        var container = new Container
        {
            Number = "MSCU1234567",
            IsoType = ContainerIsoType.Hc40,
            LastFreeDay = new DateOnly(2026, 10, 15),
            DemurrageRatePerDay = 150m,
            PickedUpAt = Now.AddDays(-1),
            ChassisProvider = ChassisProvider.Own,
            ChassisRatePerDay = 30m
        };

        var accessorials = AddDrayageChargesHandler.BuildAccessorials(
            LoadInvoiceChargesTests.NewLoad(), container, container.GetFreeTime(Now));

        Assert.Empty(accessorials);
    }

    [Fact]
    public async Task Handle_RunAgain_ReplacesItsOwnAccessorialsAndSyncsInvoice()
    {
        var container = OverdueContainer();
        var load = LoadInvoiceChargesTests.NewLoad();
        load.ContainerId = container.Id;
        var lumper = new LoadAccessorial
        {
            LoadId = load.Id, Load = load, Name = "Lumper", LineItemType = InvoiceLineItemType.Lumper,
            Unit = AccessorialUnit.Flat, Rate = 100m
        };
        var oldDemurrage = new LoadAccessorial
        {
            LoadId = load.Id, Load = load, Name = "Demurrage", LineItemType = InvoiceLineItemType.Demurrage,
            Unit = AccessorialUnit.PerDay, Rate = 150m
        };
        load.Accessorials.AddRange([lumper, oldDemurrage]);

        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        var loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
        var containerRepo = Substitute.For<ITenantRepository<Container, Guid>>();
        tenantUow.Repository<Load>().Returns(loadRepo);
        tenantUow.Repository<Container>().Returns(containerRepo);
        loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);
        containerRepo.GetByIdAsync(container.Id, Arg.Any<CancellationToken>()).Returns(container);
        var loadCharges = Substitute.For<ILoadChargeService>();
        loadCharges.SyncInvoiceAsync(load, Arg.Any<CancellationToken>()).Returns(true);

        var result = await new AddDrayageChargesHandler(tenantUow, loadCharges)
            .Handle(new AddDrayageChargesCommand { LoadId = load.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await loadCharges.Received(1).ReplaceAccessorialsAsync(
            load,
            Arg.Is<Func<LoadAccessorial, bool>>(owns => owns(oldDemurrage) && !owns(lumper)),
            Arg.Is<IReadOnlyList<LoadAccessorial>>(fresh => fresh.Count(a => a.LineItemType == InvoiceLineItemType.Demurrage) == 1),
            Arg.Any<CancellationToken>());
        await loadCharges.Received(1).SyncInvoiceAsync(load, Arg.Any<CancellationToken>());
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
