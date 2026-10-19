using System.Linq.Expressions;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class BulkAssignLoadsHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly List<Load> _loads = [];
    private readonly BulkAssignLoadsHandler _sut;

    public BulkAssignLoadsHandlerTests()
    {
        var repo = Substitute.For<ITenantRepository<Load, Guid>>();
        repo.Query().Returns(_ => _loads.BuildMock());
        repo.GetListAsync(Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => _loads.Where(ci.Arg<Expression<Func<Load, bool>>>().Compile()).ToList());
        _tenantUow.Repository<Load>().Returns(repo);
        _sut = new BulkAssignLoadsHandler(_tenantUow);
    }

    [Fact]
    public async Task Handle_LoadsOffTrip_AssignsEveryLoad()
    {
        _loads.AddRange([LoadInvoiceChargesTests.NewLoad(), LoadInvoiceChargesTests.NewLoad()]);
        var truckId = Guid.NewGuid();

        var result = await Handle(truckId);

        Assert.True(result.IsSuccess);
        Assert.All(_loads, l => Assert.Equal(truckId, l.AssignedTruckId));
    }

    [Fact]
    public async Task Handle_OneLoadOnTrip_FailsAndAssignsNone()
    {
        var free = LoadInvoiceChargesTests.NewLoad();
        var onTrip = LoadInvoiceChargesTests.NewLoad();
        onTrip.TripStops.Add(Trip.Create("Trip", null, [onTrip]).Stops[0]);
        _loads.AddRange([free, onTrip]);

        var result = await Handle(Guid.NewGuid());

        Assert.Contains("Change the trip's truck instead.", result.Error);
        Assert.All(_loads, l => Assert.Null(l.AssignedTruckId));
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private Task<Shared.Models.Result> Handle(Guid truckId) => _sut.Handle(
        new BulkAssignLoadsCommand { LoadIds = _loads.Select(l => l.Id).ToArray(), TruckId = truckId },
        CancellationToken.None);
}
