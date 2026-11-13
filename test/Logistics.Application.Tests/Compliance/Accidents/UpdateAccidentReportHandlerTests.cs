using Logistics.Application.Modules.Compliance.Accidents.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance.Accidents;

public class UpdateAccidentReportHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly Truck _truck = TestData.Truck();
    private readonly Employee _driver = TestData.Driver();
    private readonly AccidentReport _report;
    private readonly UpdateAccidentReportHandler _sut;

    public UpdateAccidentReportHandlerTests()
    {
        _report = new AccidentReport
        {
            TruckId = _truck.Id,
            Truck = _truck,
            DriverId = _driver.Id,
            Driver = _driver,
            Status = AccidentReportStatus.Draft,
            AccidentType = AccidentType.Collision,
            Severity = AccidentSeverity.Minor,
            AccidentDateTime = DateTime.UtcNow,
            Latitude = 0,
            Longitude = 0,
            FatalitiesReported = true,
            NumberOfFatalities = 1
        };

        _tenantUow.Stub(_report);
        _tenantUow.Stub(_truck);
        _tenantUow.Stub(_driver);
        _sut = new UpdateAccidentReportHandler(_tenantUow);
    }

    [Fact]
    public async Task Handle_FatalitiesTowAndHazmat_PersistsAndReturnsThem()
    {
        var command = Command();
        command.FatalitiesReported = true;
        command.NumberOfFatalities = 2;
        command.VehicleTowed = true;
        command.TowCompany = "Ace Towing";
        command.HazmatInvolved = true;
        command.HazmatDescription = "Diesel spill";

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(_report.FatalitiesReported);
        Assert.Equal(2, _report.NumberOfFatalities);
        Assert.True(_report.VehicleTowed);
        Assert.Equal("Ace Towing", _report.TowCompany);
        Assert.True(_report.HazmatInvolved);
        Assert.Equal("Diesel spill", _report.HazmatDescription);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        var dto = result.Value!;
        Assert.Equal(2, dto.NumberOfFatalities);
        Assert.Equal("Ace Towing", dto.TowCompany);
        Assert.Equal("Diesel spill", dto.HazmatDescription);
    }

    [Fact]
    public async Task Handle_FatalitiesNoLongerReported_ClearsTheCount()
    {
        var command = Command();
        command.FatalitiesReported = false;
        command.NumberOfFatalities = 1;

        await _sut.Handle(command, CancellationToken.None);

        Assert.False(_report.FatalitiesReported);
        Assert.Null(_report.NumberOfFatalities);
    }

    private UpdateAccidentReportCommand Command() => new()
    {
        Id = _report.Id,
        TruckId = _truck.Id,
        DriverId = _driver.Id,
        Type = AccidentType.Collision,
        Severity = AccidentSeverity.Minor,
        AccidentDateTime = DateTime.UtcNow,
        Location = "I-35, Dallas, TX",
        Description = "Rear-ended at a light"
    };
}
