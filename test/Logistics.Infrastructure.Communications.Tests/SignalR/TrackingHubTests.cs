using System.Security.Claims;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Communications.Tests.SignalR;

/// <summary>
/// The cached report is persisted on disconnect and its TenantId selects the database, so a
/// client-supplied one would be a cross-tenant write.
/// </summary>
public class TrackingHubTests
{
    private const string ConnectionId = "conn-1";

    private readonly ITruckGeolocationUpdater updater = Substitute.For<ITruckGeolocationUpdater>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly TrackingHubContext hubContext = new();
    private readonly ITrackingHubClient groupClient = Substitute.For<ITrackingHubClient>();

    private readonly Guid callerTenantId = Guid.NewGuid();
    private readonly Guid driverId = Guid.NewGuid();
    private readonly Guid truckId = Guid.NewGuid();

    private readonly TrackingHub sut;

    public TrackingHubTests()
    {
        tenantUow.Repository<Truck>().Returns(truckRepo);
        sut = new TrackingHub(updater, tenantUow, hubContext);

        var clients = Substitute.For<IHubCallerClients<ITrackingHubClient>>();
        clients.Group(Arg.Any<string>()).Returns(groupClient);

        sut.Clients = clients;
        sut.Groups = Substitute.For<IGroupManager>();
        sut.Context = CallerContext(callerTenantId, driverId);
    }

    private static HubCallerContext CallerContext(Guid tenantId, Guid userId)
    {
        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);
        context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(CustomClaimTypes.Tenant, tenantId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ])));
        return context;
    }

    private Truck DriversTruck() => new()
    {
        Id = truckId,
        Number = "101",
        Type = TruckType.FreightTruck,
        MainDriverId = driverId
    };

    private static TruckGeolocationDto Report(Guid truckId, Guid tenantId) => new()
    {
        TruckId = truckId,
        TenantId = tenantId,
        CurrentLocation = new GeoPoint(-73.9, 40.7)
    };

    [Fact]
    public async Task SendGeolocationData_SpoofedTenantId_CachesTheCallersOwnTenant()
    {
        truckRepo.GetByIdAsync(truckId, Arg.Any<CancellationToken>()).Returns(DriversTruck());
        var spoofed = Report(truckId, Guid.NewGuid());

        await sut.SendGeolocationData(spoofed);

        var cached = hubContext.GetGeolocationData(ConnectionId);
        Assert.NotNull(cached);
        Assert.Equal(callerTenantId, cached.TenantId);
    }

    [Fact]
    public async Task SendGeolocationData_TruckTheCallerDoesNotDrive_IsIgnored()
    {
        var someoneElsesTruck = new Truck
        {
            Id = truckId, Number = "102", Type = TruckType.FreightTruck, MainDriverId = Guid.NewGuid()
        };
        truckRepo.GetByIdAsync(truckId, Arg.Any<CancellationToken>()).Returns(someoneElsesTruck);

        await sut.SendGeolocationData(Report(truckId, callerTenantId));

        Assert.Null(hubContext.GetGeolocationData(ConnectionId));
        await groupClient.DidNotReceive().ReceiveGeolocationData(Arg.Any<TruckGeolocationDto>());
    }

    [Fact]
    public async Task SendGeolocationData_OwnTruck_BroadcastsToTheCallersTenantGroup()
    {
        truckRepo.GetByIdAsync(truckId, Arg.Any<CancellationToken>()).Returns(DriversTruck());

        await sut.SendGeolocationData(Report(truckId, callerTenantId));

        await groupClient.Received(1).ReceiveGeolocationData(
            Arg.Is<TruckGeolocationDto>(g => g.TenantId == callerTenantId));
    }

    [Fact]
    public async Task OnConnectedAsync_WithoutATenantClaim_AbortsTheConnection()
    {
        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);
        context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        sut.Context = context;

        await sut.OnConnectedAsync();

        context.Received(1).Abort();
    }
}
