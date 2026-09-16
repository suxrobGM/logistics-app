using Logistics.Application.Modules.Compliance.Dvir.Queries;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance;

public class GetTruckOpenDvirDefectsHandlerTests
{
    private static readonly Guid TruckId = Guid.NewGuid();

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<DvirReport, Guid> _reportRepo =
        Substitute.For<ITenantRepository<DvirReport, Guid>>();

    private readonly GetTruckOpenDvirDefectsHandler _sut;

    public GetTruckOpenDvirDefectsHandlerTests()
    {
        _tenantUow.Repository<DvirReport>().Returns(_reportRepo);
        _sut = new GetTruckOpenDvirDefectsHandler(_tenantUow);
    }

    private static DvirReport Report(
        Guid truckId, DvirType type, DvirStatus status, int daysAgo, params (string Description, bool IsCorrected)[] defects)
    {
        var report = new DvirReport
        {
            TruckId = truckId,
            Type = type,
            Status = status,
            InspectionDate = DateTime.UtcNow.AddDays(-daysAgo)
        };

        report.Defects.AddRange(defects.Select(d => new DvirDefect
        {
            DvirReport = report,
            Category = Enum.GetValues<DvirInspectionCategory>()[0],
            Severity = Enum.GetValues<DefectSeverity>()[0],
            Description = d.Description,
            IsCorrected = d.IsCorrected
        }));

        return report;
    }

    [Fact]
    public async Task Handle_ReturnsUncorrectedDefectsFromLatestReportPerType()
    {
        _reportRepo.Query().Returns(new List<DvirReport>
        {
            Report(TruckId, DvirType.PreTrip, DvirStatus.Submitted, daysAgo: 3, ("superseded", false)),
            Report(TruckId, DvirType.PreTrip, DvirStatus.RequiresRepair, daysAgo: 1, ("brake light", false), ("mirror", true)),
            Report(TruckId, DvirType.PostTrip, DvirStatus.Submitted, daysAgo: 2, ("tire tread", false)),
            Report(TruckId, DvirType.PostTrip, DvirStatus.Draft, daysAgo: 0, ("draft", false)),
            Report(Guid.NewGuid(), DvirType.PreTrip, DvirStatus.Submitted, daysAgo: 0, ("other truck", false))
        }.BuildMock());

        var result = await _sut.Handle(new GetTruckOpenDvirDefectsQuery(TruckId), CancellationToken.None);

        Assert.Equal(["brake light", "tire tread"], result.Value!.Select(d => d.Description));
    }
}
