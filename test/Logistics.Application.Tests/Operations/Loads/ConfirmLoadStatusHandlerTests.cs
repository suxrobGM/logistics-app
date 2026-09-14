using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Notifications;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class ConfirmLoadStatusHandlerTests
{
    private static readonly Guid DriverId = Guid.NewGuid();

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository<Load, Guid> loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();

    private readonly Load load;
    private readonly ConfirmLoadStatusHandler sut;

    public ConfirmLoadStatusHandlerTests()
    {
        var truck = new Truck { Number = "TRK-001", Type = TruckType.FreightTruck, MainDriverId = DriverId };
        var address = new Address { Line1 = "1 Main", City = "City", State = "ST", ZipCode = "00000", Country = "US" };
        load = new Load
        {
            Name = "Test Load",
            Type = LoadType.GeneralFreight,
            Customer = null!,
            OriginAddress = address,
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = address,
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = Money.Zero("USD"),
            AssignedTruck = truck,
            AssignedTruckId = truck.Id
        };
        load.UpdateStatus(LoadStatus.Dispatched, force: true);

        tenantUow.Repository<Load>().Returns(loadRepo);
        loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);
        currentUser.IsInRole(Arg.Is<string[]>(r => r.Contains(TenantRoles.Driver))).Returns(true);

        sut = new ConfirmLoadStatusHandler(tenantUow, currentUser, Substitute.For<INotificationService>());
    }

    private Task<Result> Confirm(LoadStatus status) =>
        sut.Handle(new ConfirmLoadStatusCommand { LoadId = load.Id, LoadStatus = status }, CancellationToken.None);

    [Fact]
    public async Task Handle_DriverNotOnTruck_IsRejected()
    {
        currentUser.GetUserId().Returns(Guid.NewGuid());

        var result = await Confirm(LoadStatus.PickedUp);

        Assert.Equal("This load isn't assigned to your truck.", result.Error);
        Assert.Equal(LoadStatus.Dispatched, load.Status);
    }

    [Fact]
    public async Task Handle_SkippingPickup_IsRejected()
    {
        currentUser.GetUserId().Returns(DriverId);

        var result = await Confirm(LoadStatus.Delivered);

        Assert.Equal("This load is Dispatched, so it can't be marked Delivered.", result.Error);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AssignedDriverConfirmsPickup_UpdatesStatus()
    {
        currentUser.GetUserId().Returns(DriverId);

        var result = await Confirm(LoadStatus.PickedUp);

        Assert.True(result.IsSuccess);
        Assert.Equal(LoadStatus.PickedUp, load.Status);
    }
}
