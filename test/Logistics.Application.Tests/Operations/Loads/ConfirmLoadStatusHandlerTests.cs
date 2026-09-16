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

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository<Load, Guid> _loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();

    private readonly Load _load;
    private readonly ConfirmLoadStatusHandler _sut;

    public ConfirmLoadStatusHandlerTests()
    {
        var truck = new Truck { Number = "TRK-001", Type = TruckType.FreightTruck, MainDriverId = DriverId };
        var address = new Address { Line1 = "1 Main", City = "City", State = "ST", ZipCode = "00000", Country = "US" };
        _load = new Load
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
        _load.UpdateStatus(LoadStatus.Dispatched, force: true);

        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _loadRepo.GetByIdAsync(_load.Id, Arg.Any<CancellationToken>()).Returns(_load);
        _currentUser.IsInRole(Arg.Is<string[]>(r => r.Contains(TenantRoles.Driver))).Returns(true);

        _sut = new ConfirmLoadStatusHandler(_tenantUow, _currentUser, Substitute.For<INotificationService>());
    }

    private Task<Result> Confirm(LoadStatus status) =>
        _sut.Handle(new ConfirmLoadStatusCommand { LoadId = _load.Id, LoadStatus = status }, CancellationToken.None);

    [Fact]
    public async Task Handle_DriverNotOnTruck_IsRejected()
    {
        _currentUser.GetUserId().Returns(Guid.NewGuid());

        var result = await Confirm(LoadStatus.PickedUp);

        Assert.Equal("This load isn't assigned to your truck.", result.Error);
        Assert.Equal(LoadStatus.Dispatched, _load.Status);
    }

    [Fact]
    public async Task Handle_SkippingPickup_IsRejected()
    {
        _currentUser.GetUserId().Returns(DriverId);

        var result = await Confirm(LoadStatus.Delivered);

        Assert.Equal("This load is Dispatched, so it can't be marked Delivered.", result.Error);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AssignedDriverConfirmsPickup_UpdatesStatus()
    {
        _currentUser.GetUserId().Returns(DriverId);

        var result = await Confirm(LoadStatus.PickedUp);

        Assert.True(result.IsSuccess);
        Assert.Equal(LoadStatus.PickedUp, _load.Status);
    }
}
