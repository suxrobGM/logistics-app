using Logistics.Application.Common.Services;
using Logistics.Application.Modules.Compliance.Dvir.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance.Dvir;

public class DvirReportListMapperTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IDisplayNameLookup _names = Substitute.For<IDisplayNameLookup>();
    private readonly DvirReportListMapper _sut;

    public DvirReportListMapperTests()
    {
        _sut = new DvirReportListMapper(_tenantUow, _names);
    }

    /// <summary>Navigations stay unset, so a lazy read would come back empty.</summary>
    [Fact]
    public async Task MapAsync_FillsNamesAndDefectsFromThePageLookups()
    {
        var driverId = Guid.NewGuid();
        var mechanicId = Guid.NewGuid();
        var first = Report(driverId);
        var second = Report(driverId);
        var defect = new DvirDefect
        {
            DvirReportId = second.Id,
            Category = Enum.GetValues<DvirInspectionCategory>()[0],
            Severity = Enum.GetValues<DefectSeverity>()[0],
            Description = "Cracked mirror",
            CorrectedById = mechanicId
        };
        _tenantUow.Stub<DvirDefect>(defect);
        _names.GetEmployeeNamesAsync(Arg.Any<IEnumerable<Guid?>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [driverId] = "Dana Driver", [mechanicId] = "Max Mechanic" });
        _names.GetTruckNumbersAsync(Arg.Any<IEnumerable<Guid?>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [first.TruckId] = "T-1", [second.TruckId] = "T-2" });

        var dtos = (await _sut.MapAsync([first, second], CancellationToken.None)).ToList();

        Assert.Equal(["T-1", "T-2"], dtos.Select(d => d.TruckNumber));
        Assert.All(dtos, d => Assert.Equal("Dana Driver", d.DriverName));
        Assert.Empty(dtos[0].Defects);
        Assert.Equal("Max Mechanic", Assert.Single(dtos[1].Defects).CorrectedByName);
    }

    private static DvirReport Report(Guid driverId) => new()
    {
        TruckId = Guid.NewGuid(),
        DriverId = driverId,
        Type = Enum.GetValues<DvirType>()[0],
        Status = DvirStatus.Submitted,
        InspectionDate = DateTime.UtcNow
    };
}
