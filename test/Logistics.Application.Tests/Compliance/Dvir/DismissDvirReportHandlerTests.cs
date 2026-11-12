using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Compliance.Dvir.Commands;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance.Dvir;

public class DismissDvirReportHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<DvirReport, Guid> _reportRepo = Substitute.For<ITenantRepository<DvirReport, Guid>>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly DismissDvirReportHandler _sut;

    public DismissDvirReportHandlerTests()
    {
        _tenantUow.Repository<DvirReport>().Returns(_reportRepo);
        _sut = new DismissDvirReportHandler(_tenantUow, _currentUser);
    }

    /// <summary>The reviewer is an audit field, so it comes from the caller's token, never the request body.</summary>
    [Fact]
    public async Task Handle_RecordsTheCallerAsReviewer()
    {
        var callerId = Guid.NewGuid();
        _currentUser.GetUserId().Returns(callerId);
        var report = new DvirReport
        {
            TruckId = Guid.NewGuid(),
            Type = Enum.GetValues<DvirType>()[0],
            Status = DvirStatus.Submitted,
            InspectionDate = DateTime.UtcNow
        };
        _reportRepo.GetByIdAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);

        var result = await _sut.Handle(new DismissDvirReportCommand { ReportId = report.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(callerId, report.ReviewedById);
        Assert.Equal(DvirStatus.Cleared, report.Status);
    }

    [Fact]
    public async Task Handle_NoCurrentUser_FailsWithoutSaving()
    {
        _currentUser.GetUserId().Returns((Guid?)null);
        var report = new DvirReport
        {
            TruckId = Guid.NewGuid(),
            Type = Enum.GetValues<DvirType>()[0],
            Status = DvirStatus.Submitted,
            InspectionDate = DateTime.UtcNow
        };
        _reportRepo.GetByIdAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);

        var result = await _sut.Handle(new DismissDvirReportCommand { ReportId = report.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
