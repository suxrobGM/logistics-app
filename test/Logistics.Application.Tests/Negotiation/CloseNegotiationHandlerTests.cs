using System.Linq.Expressions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class CloseNegotiationHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly ITenantRepository<RateNegotiation, Guid> negotiationRepo =
        Substitute.For<ITenantRepository<RateNegotiation, Guid>>();
    private readonly ITenantRepository<LoadBoardListing, Guid> listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly IMasterRepository<InboundEmailRoute, Guid> routeRepo =
        Substitute.For<IMasterRepository<InboundEmailRoute, Guid>>();

    private readonly Tenant tenant;
    private readonly RateNegotiation negotiation;
    private readonly CloseNegotiationHandler sut;

    public CloseNegotiationHandlerTests()
    {
        tenant = new Tenant
        {
            Name = "test",
            ConnectionString = "test",
            BillingEmail = "billing@test.com",
            CompanyAddress = new Address
            {
                Line1 = "1 Test St", City = "Test", State = "TX", ZipCode = "00000", Country = "US"
            }
        };

        negotiation = RateNegotiation.Create(Guid.NewGuid(), "broker@example.com");

        tenantUow.Repository<RateNegotiation>().Returns(negotiationRepo);
        tenantUow.Repository<LoadBoardListing>().Returns(listingRepo);
        tenantUow.GetCurrentTenant().Returns(tenant);
        masterUow.Repository<InboundEmailRoute>().Returns(routeRepo);

        negotiationRepo.GetByIdAsync(negotiation.Id, Arg.Any<CancellationToken>()).Returns(negotiation);

        sut = new CloseNegotiationHandler(tenantUow, masterUow, broadcastService);
    }

    private CloseNegotiationCommand Command() => new() { Id = negotiation.Id, Reason = "Broker went quiet" };

    private void SetupRoute(InboundEmailRoute? route) =>
        routeRepo.GetAsync(Arg.Any<Expression<Func<InboundEmailRoute, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(route);

    [Fact]
    public async Task Handle_ActiveThread_ClosesAndRevokesRoute()
    {
        var route = new InboundEmailRoute { ThreadToken = negotiation.ReplyToken, TenantId = tenant.Id };
        SetupRoute(route);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.Closed, negotiation.Status);
        Assert.Equal("Broker went quiet", negotiation.CloseReason);
        Assert.NotNull(negotiation.ClosedAt);
        Assert.NotNull(route.RevokedAt);
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await broadcastService.Received(1).BroadcastNegotiationAsync(tenant.Id, Arg.Any<RateNegotiationDto>());
    }

    [Fact]
    public async Task Handle_Declined_ClosesAsDeclined()
    {
        SetupRoute(null);
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
