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
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IInboundEmailRouteRegistry routeRegistry = Substitute.For<IInboundEmailRouteRegistry>();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly ITenantRepository<RateNegotiation, Guid> negotiationRepo =
        Substitute.For<ITenantRepository<RateNegotiation, Guid>>();
    private readonly ITenantRepository<LoadBoardListing, Guid> listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly Tenant tenant;
    private readonly RateNegotiation negotiation;
    private readonly CloseNegotiationHandler sut;

    public CloseNegotiationHandlerTests()
    {
        tenant = TestTenant.Create();

        negotiation = RateNegotiation.Create(Guid.NewGuid(), "broker@example.com", RateFloorSnapshot.None);

        tenantUow.Repository<RateNegotiation>().Returns(negotiationRepo);
        tenantUow.Repository<LoadBoardListing>().Returns(listingRepo);
        tenantUow.GetCurrentTenant().Returns(tenant);

        negotiationRepo.GetByIdAsync(negotiation.Id, Arg.Any<CancellationToken>()).Returns(negotiation);

        sut = new CloseNegotiationHandler(tenantUow, routeRegistry, broadcastService);
    }

    private CloseNegotiationCommand Command() => new() { Id = negotiation.Id, Reason = "Broker went quiet" };

    [Fact]
    public async Task Handle_ActiveThread_ClosesAndRevokesRoute()
    {
        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.Closed, negotiation.Status);
        Assert.Equal("Broker went quiet", negotiation.CloseReason);
        Assert.NotNull(negotiation.ClosedAt);
        await routeRegistry.Received(1).RevokeAsync(
            Arg.Is<IEnumerable<string>>(t => t.Single() == negotiation.ReplyToken),
            Arg.Any<CancellationToken>());
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await broadcastService.Received(1).BroadcastNegotiationAsync(tenant.Id, Arg.Any<RateNegotiationDto>());
    }

    [Fact]
    public async Task Handle_Declined_ClosesAsDeclined()
    {
        var command = Command();
        command.Declined = true;

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.Declined, negotiation.Status);
    }

    [Fact]
    public async Task Handle_AlreadyClosed_Fails()
    {
        negotiation.Close(RateNegotiationStatus.Closed);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_Fails()
    {
        negotiationRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RateNegotiation?)null);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
