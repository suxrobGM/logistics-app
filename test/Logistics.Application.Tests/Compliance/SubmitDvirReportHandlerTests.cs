using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Compliance.Dvir.Commands;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance;

public class SubmitDvirReportHandlerTests
{
    private static readonly Guid DriverId = Guid.NewGuid();

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository<DvirReport, Guid> reportRepo =
        Substitute.For<ITenantRepository<DvirReport, Guid>>();

    private readonly SubmitDvirReportHandler sut;

    public SubmitDvirReportHandlerTests()
    {
        tenantUow.Repository<DvirReport>().Returns(reportRepo);
        currentUser.GetUserId().Returns(DriverId);
        sut = new SubmitDvirReportHandler(tenantUow, currentUser);
    }

    [Fact]
    public async Task Handle_AlreadySubmitted_ReturnsCurrentReportWithoutSaving()
    {
        var report = new DvirReport { DriverId = DriverId, Type = DvirType.PreTrip, Status = DvirStatus.Submitted };
        reportRepo.GetByIdAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);

        var result = await sut.Handle(new SubmitDvirReportCommand { ReportId = report.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(report.Id, result.Value!.Id);
        Assert.Equal(DvirStatus.Submitted, result.Value.Status);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
