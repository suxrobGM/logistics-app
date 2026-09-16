using System.Linq.Expressions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Application.Tests.TestKit;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class CloseNegotiationHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IInboundEmailRouteRegistry _routeRegistry = Substitute.For<IInboundEmailRouteRegistry>();
    private readonly IAIDispatchBroadcastService _broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly ITenantRepository<RateNegotiation, Guid> _negotiationRepo =
        Substitute.For<ITenantRepository<RateNegotiation, Guid>>();
    private readonly ITenantRepository<LoadBoardListing, Guid> _listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly Tenant _tenant;
    private readonly RateNegotiation _negotiation;
    private readonly CloseNegotiationHandler _sut;

    public CloseNegotiationHandlerTests()
    {
        _tenant = TestTenant.Create();

        _negotiation = RateNegotiation.Create(Guid.NewGuid(), "broker@example.com", RateFloorSnapshot.None);

        _tenantUow.Repository<RateNegotiation>().Returns(_negotiationRepo);
        _tenantUow.Repository<LoadBoardListing>().Returns(_listingRepo);
        _tenantUow.GetCurrentTenant().Returns(_tenant);

        _negotiationRepo.GetByIdAsync(_negotiation.Id, Arg.Any<CancellationToken>()).Returns(_negotiation);

        _sut = new CloseNegotiationHandler(_tenantUow, _routeRegistry, _broadcastService);
    }

    private CloseNegotiationCommand Command() => new() { Id = _negotiation.Id, Reason = "Broker went quiet" };

    [Fact]
    public async Task Handle_ActiveThread_ClosesAndRevokesRoute()
    {
        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.Closed, _negotiation.Status);
        Assert.Equal("Broker went quiet", _negotiation.CloseReason);
        Assert.NotNull(_negotiation.ClosedAt);
        await _routeRegistry.Received(1).RevokeAsync(
            Arg.Is<IEnumerable<string>>(t => t.Single() == _negotiation.ReplyToken),
            Arg.Any<CancellationToken>());
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _broadcastService.Received(1).BroadcastNegotiationAsync(_tenant.Id, Arg.Any<RateNegotiationDto>());
    }

    [Fact]
    public async Task Handle_Declined_ClosesAsDeclined()
    {
        var command = Command();
        command.Declined = true;

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.Declined, _negotiation.Status);
    }

    [Fact]
    public async Task Handle_AlreadyClosed_Fails()
    {
        _negotiation.Close(RateNegotiationStatus.Closed);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_Fails()
    {
        _negotiationRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RateNegotiation?)null);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
