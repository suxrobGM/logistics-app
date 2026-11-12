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
    private const string ConnectionId = HubTestContext.ConnectionId;

    private readonly ITruckGeolocationUpdater _updater = Substitute.For<ITruckGeolocationUpdater>();
    private readonly TrackingHubContext _hubContext = new();
    private readonly ITrackingHubClient _groupClient = Substitute.For<ITrackingHubClient>();

    private readonly Guid _callerTenantId = Guid.NewGuid();
    private readonly Guid _driverId = Guid.NewGuid();
    private readonly Guid _truckId = Guid.NewGuid();

    private readonly TrackingHub _sut;

    public TrackingHubTests()
    {
        _sut = new TrackingHub(_updater, _hubContext);

        var clients = Substitute.For<IHubCallerClients<ITrackingHubClient>>();
        clients.Group(Arg.Any<string>()).Returns(_groupClient);

        _sut.Clients = clients;
        _sut.Groups = Substitute.For<IGroupManager>();
        _sut.Context = HubTestContext.Caller(_callerTenantId, _driverId);
    }

    private void AllowReporting(bool allowed) =>
        _updater.CanDriverReportForTruckAsync(
                _callerTenantId, _truckId, _driverId, Arg.Any<CancellationToken>())
            .Returns(allowed);

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

        await _sut.SendGeolocationData(Report(_truckId, Guid.NewGuid()));

        var cached = _hubContext.GetGeolocationData(ConnectionId);
        Assert.NotNull(cached);
        Assert.Equal(_callerTenantId, cached.TenantId);
    }

    [Fact]
    public async Task SendGeolocationData_TruckTheCallerDoesNotDrive_IsIgnored()
    {
        AllowReporting(false);

        await _sut.SendGeolocationData(Report(_truckId, _callerTenantId));

        Assert.Null(_hubContext.GetGeolocationData(ConnectionId));
        await _groupClient.DidNotReceive().ReceiveGeolocationData(Arg.Any<TruckGeolocationDto>());
    }

    [Fact]
    public async Task SendGeolocationData_OwnTruck_BroadcastsToTheCallersTenantGroup()
    {
        AllowReporting(true);

        await _sut.SendGeolocationData(Report(_truckId, _callerTenantId));

        await _groupClient.Received(1).ReceiveGeolocationData(
            Arg.Is<TruckGeolocationDto>(g => g.TenantId == _callerTenantId));
    }

    [Fact]
    public async Task OnConnectedAsync_WithoutATenantClaim_AbortsTheConnection()
    {
        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);
        context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        _sut.Context = context;

        await _sut.OnConnectedAsync();

        context.Received(1).Abort();
    }

    [Fact]
    public async Task OnConnectedAsync_WithoutAUserClaim_AbortsTheConnection()
    {
        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);
        context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(CustomClaimTypes.Tenant, _callerTenantId.ToString())])));
        _sut.Context = context;

        await _sut.OnConnectedAsync();

        context.Received(1).Abort();
    }
}
