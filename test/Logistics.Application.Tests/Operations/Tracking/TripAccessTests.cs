using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Tracking;

public class TripAccessTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid TripId = Guid.NewGuid();
    private static readonly Guid TruckId = Guid.NewGuid();
    private static readonly Guid CallerId = Guid.NewGuid();

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IUserPermissionService userPermissions = Substitute.For<IUserPermissionService>();

    private readonly ITenantRepository<Trip, Guid> tripRepo = Substitute.For<ITenantRepository<Trip, Guid>>();
    private readonly ITenantRepository<Truck, Guid> truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();

    private readonly TripAccess sut;

    public TripAccessTests()
    {
        tenantUow.Repository<Trip>().Returns(tripRepo);
        tenantUow.Repository<Truck>().Returns(truckRepo);
        sut = new TripAccess(tenantUow, userPermissions);
    }

    private void GivenTrip(Guid? truckId) =>
        tripRepo.GetByIdAsync(TripId, Arg.Any<CancellationToken>())
            .Returns(new Trip { Id = TripId, Name = "T-1", TruckId = truckId });

    private void GivenTruck(Guid? mainDriverId, Guid? secondaryDriverId = null) =>
        truckRepo.GetByIdAsync(TruckId, Arg.Any<CancellationToken>())
            .Returns(new Truck
            {
                Id = TruckId,
                Number = "TR-1",
                Type = TruckType.ContainerTruck,
                MainDriverId = mainDriverId,
                SecondaryDriverId = secondaryDriverId
            });

    private void GivenDispatchPermission(bool granted) =>
        userPermissions
            .HasPermissionAsync(CallerId, TenantId, Permission.Load.Manage, Arg.Any<CancellationToken>())
            .Returns(granted);

    [Fact]
    public async Task CanUserViewTrip_ResolvesAgainstTheCallersOwnTenant()
    {
        GivenTrip(TruckId);
        GivenDispatchPermission(true);

        await sut.CanUserViewTripAsync(TenantId, TripId, CallerId);

        await tenantUow.Received(1).SetCurrentTenantByIdAsync(TenantId);
    }

    [Fact]
    public async Task CanUserViewTrip_TripNotInTheCallersTenant_IsDenied()
    {
        tripRepo.GetByIdAsync(TripId, Arg.Any<CancellationToken>()).Returns((Trip?)null);
        GivenDispatchPermission(true);

        Assert.False(await sut.CanUserViewTripAsync(TenantId, TripId, CallerId));
    }

    [Fact]
    public async Task CanUserViewTrip_Dispatch_MayViewAnyTripInTheirTenant()
    {
        GivenTrip(TruckId);
        GivenTruck(mainDriverId: Guid.NewGuid());
        GivenDispatchPermission(true);

        Assert.True(await sut.CanUserViewTripAsync(TenantId, TripId, CallerId));
    }

    [Fact]
    public async Task CanUserViewTrip_Dispatch_MayViewAnUnassignedTrip()
    {
        GivenTrip(truckId: null);
        GivenDispatchPermission(true);

        Assert.True(await sut.CanUserViewTripAsync(TenantId, TripId, CallerId));
    }

    [Fact]
    public async Task CanUserViewTrip_MainDriverOfTheAssignedTruck_IsAllowed()
    {
        GivenTrip(TruckId);
        GivenTruck(mainDriverId: CallerId);
        GivenDispatchPermission(false);

        Assert.True(await sut.CanUserViewTripAsync(TenantId, TripId, CallerId));
    }

    [Fact]
    public async Task CanUserViewTrip_SecondDriverOfTheAssignedTruck_IsAllowed()
    {
        // Team driving is a real case on this entity. Both seats run the trip.
        GivenTrip(TruckId);
        GivenTruck(mainDriverId: Guid.NewGuid(), secondaryDriverId: CallerId);
        GivenDispatchPermission(false);

        Assert.True(await sut.CanUserViewTripAsync(TenantId, TripId, CallerId));
    }

    // Before this, any employee of the tenant passed.
    [Fact]
    public async Task CanUserViewTrip_EmployeeWhoNeitherDispatchesNorDrivesIt_IsDenied()
    {
        GivenTrip(TruckId);
        GivenTruck(mainDriverId: Guid.NewGuid(), secondaryDriverId: Guid.NewGuid());
        GivenDispatchPermission(false);

        Assert.False(await sut.CanUserViewTripAsync(TenantId, TripId, CallerId));
    }

    [Fact]
    public async Task CanUserViewTrip_NonDispatchCallerOnAnUnassignedTrip_IsDenied()
    {
        GivenTrip(truckId: null);
        GivenDispatchPermission(false);

        Assert.False(await sut.CanUserViewTripAsync(TenantId, TripId, CallerId));
        await truckRepo.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
    }

    [Fact]
    public async Task CanUserViewTrip_AssignedTruckRowMissing_IsDenied()
    {
        GivenTrip(TruckId);
        truckRepo.GetByIdAsync(TruckId, Arg.Any<CancellationToken>()).Returns((Truck?)null);
        GivenDispatchPermission(false);

        Assert.False(await sut.CanUserViewTripAsync(TenantId, TripId, CallerId));
    }

    [Fact]
    public async Task CanUserViewTrip_ChecksThePermissionAgainstTheCallersOwnTenant()
    {
        // Permissions resolved against the wrong tenant would be the bug again.
        GivenTrip(TruckId);
        GivenTruck(mainDriverId: CallerId);
        GivenDispatchPermission(false);

        await sut.CanUserViewTripAsync(TenantId, TripId, CallerId);

        await userPermissions.Received(1).HasPermissionAsync(
            CallerId, TenantId, Permission.Load.Manage, Arg.Any<CancellationToken>());
    }
}
