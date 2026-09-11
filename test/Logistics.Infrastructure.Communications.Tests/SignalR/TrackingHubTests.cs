using System.Security.Claims;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Communications.Tests.SignalR;

public class TrackingHubTests
{
    private const string ConnectionId = "conn-1";

    private readonly ITruckGeolocationUpdater updater = Substitute.For<ITruckGeolocationUpdater>();
    private readonly ITripAccess tripAccess = Substitute.For<ITripAccess>();
    private readonly TrackingHubContext hubContext = new();
    private readonly ITrackingHubClient groupClient = Substitute.For<ITrackingHubClient>();
    private readonly IGroupManager groups = Substitute.For<IGroupManager>();

    private readonly Guid callerTenantId = Guid.NewGuid();
    private readonly Guid driverId = Guid.NewGuid();
    private readonly Guid truckId = Guid.NewGuid();
    private readonly Guid tripId = Guid.NewGuid();

    private readonly TrackingHub sut;

    public TrackingHubTests()
    {
        sut = new TrackingHub(updater, tripAccess, hubContext);

        var clients = Substitute.For<IHubCallerClients<ITrackingHubClient>>();
        clients.Group(Arg.Any<string>()).Returns(groupClient);

        sut.Clients = clients;
        sut.Groups = groups;
        sut.Context = CallerContext(callerTenantId, driverId);
    }

    private void AllowTripAccess(bool allowed) =>
        tripAccess.CanUserViewTripAsync(callerTenantId, tripId, driverId, Arg.Any<CancellationToken>())
            .Returns(allowed);

    private void AllowReporting(bool allowed) =>
        updater.CanDriverReportForTruckAsync(
                callerTenantId, truckId, driverId, Arg.Any<CancellationToken>())
            .Returns(allowed);

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

    private static TruckGeolocationDto Report(Guid truckId, Guid tenantId) => new()
    {
        TruckId = truckId,
        TenantId = tenantId,
        CurrentLocation = new GeoPoint(-73.9, 40.7)
    };

    [Fact]
    public async Task SendGeolocationData_SpoofedTenantId_CachesTheCallersOwnTenant()
    {
        AllowReporting(true);

        await sut.SendGeolocationData(Report(truckId, Guid.NewGuid()));

        var cached = hubContext.GetGeolocationData(ConnectionId);
        Assert.NotNull(cached);
        Assert.Equal(callerTenantId, cached.TenantId);
    }

    [Fact]
    public async Task SendGeolocationData_TruckTheCallerDoesNotDrive_IsIgnored()
    {
        AllowReporting(false);

        await sut.SendGeolocationData(Report(truckId, callerTenantId));

        Assert.Null(hubContext.GetGeolocationData(ConnectionId));
        await groupClient.DidNotReceive().ReceiveGeolocationData(Arg.Any<TruckGeolocationDto>());
    }

    [Fact]
    public async Task SendGeolocationData_OwnTruck_BroadcastsToTheCallersTenantGroup()
    {
        AllowReporting(true);

        await sut.SendGeolocationData(Report(truckId, callerTenantId));

        await groupClient.Received(1).ReceiveGeolocationData(
            Arg.Is<TruckGeolocationDto>(g => g.TenantId == callerTenantId));
    }

    // Any authenticated user could previously subscribe to any trip group by id.
    [Fact]
    public async Task SubscribeToTrip_CallerNotAuthorizedForTheTrip_DoesNotJoinTheGroup()
    {
        AllowTripAccess(false);

        await sut.SubscribeToTrip(tripId.ToString());

        await groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubscribeToTrip_AuthorizedCaller_JoinsTheGroup()
    {
        AllowTripAccess(true);

        await sut.SubscribeToTrip(tripId.ToString());

        await groups.Received(1).AddToGroupAsync(
            ConnectionId, $"trip:{tripId}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubscribeToTrip_ChecksAccessForTheCallersOwnTenantAndIdentity()
    {
        AllowTripAccess(true);

        await sut.SubscribeToTrip(tripId.ToString());

        // Both come from the caller's own claims, not from the client.
        await tripAccess.Received(1).CanUserViewTripAsync(
            callerTenantId, tripId, driverId, Arg.Any<CancellationToken>());
    }

    // Broadcasts format the group name from a Guid, so a raw string joins a group nothing targets.
    [Fact]
    public async Task SubscribeToTrip_NonCanonicalTripId_JoinsTheGroupBroadcastsActuallyTarget()
    {
        AllowTripAccess(true);

        await sut.SubscribeToTrip(tripId.ToString("B").ToUpperInvariant());

        await groups.Received(1).AddToGroupAsync(
            ConnectionId, $"trip:{tripId}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubscribeToTrip_MalformedTripId_DoesNotThrowOrJoin()
    {
        await sut.SubscribeToTrip("not-a-guid");

        await groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnsubscribeFromTrip_NonCanonicalTripId_LeavesTheGroupItActuallyJoined()
    {
        await sut.UnsubscribeFromTrip(tripId.ToString("B").ToUpperInvariant());

        await groups.Received(1).RemoveFromGroupAsync(
            ConnectionId, $"trip:{tripId}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnsubscribeFromTrip_MalformedTripId_DoesNothing()
    {
        await sut.UnsubscribeFromTrip("not-a-guid");

        await groups.DidNotReceive().RemoveFromGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
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

    [Fact]
    public async Task OnConnectedAsync_WithoutAUserClaim_AbortsTheConnection()
    {
        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);
        context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(CustomClaimTypes.Tenant, callerTenantId.ToString())])));
        sut.Context = context;

        await sut.OnConnectedAsync();

        context.Received(1).Abort();
    }
}
