using Logistics.Application.Modules.Compliance.Safety.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance.Safety;

public class CreateAccidentReportHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<AccidentReport, Guid> _reportRepo;
    private readonly Truck _truck = TestData.Truck();
    private readonly Employee _driver = TestData.Driver();
    private readonly CreateAccidentReportHandler _sut;

    public CreateAccidentReportHandlerTests()
    {
        _tenantUow.Stub(_truck);
        _tenantUow.Stub(_driver);
        _reportRepo = _tenantUow.Stub<AccidentReport>();
        _sut = new CreateAccidentReportHandler(_tenantUow);
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
        AccidentReport? added = null;
        await _reportRepo.AddAsync(Arg.Do<AccidentReport>(r => added = r), Arg.Any<CancellationToken>());

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _reportRepo.Received(1).AddAsync(Arg.Any<AccidentReport>(), Arg.Any<CancellationToken>());
        Assert.NotNull(added);
        Assert.True(added.FatalitiesReported);
        Assert.Equal(2, added.NumberOfFatalities);
        Assert.True(added.VehicleTowed);
        Assert.Equal("Ace Towing", added.TowCompany);
        Assert.True(added.HazmatInvolved);
        Assert.Equal("Diesel spill", added.HazmatDescription);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        var dto = result.Value!;
        Assert.True(dto.FatalitiesReported);
        Assert.Equal(2, dto.NumberOfFatalities);
        Assert.True(dto.VehicleTowed);
        Assert.Equal("Ace Towing", dto.TowCompany);
        Assert.True(dto.HazmatInvolved);
        Assert.Equal("Diesel spill", dto.HazmatDescription);
    }

    [Fact]
    public async Task Handle_FatalitiesNotReported_DropsTheCount()
    {
        var command = Command();
        command.FatalitiesReported = false;
        command.NumberOfFatalities = 3;

        await _sut.Handle(command, CancellationToken.None);

        await _reportRepo.Received(1).AddAsync(
            Arg.Is<AccidentReport>(r => !r.FatalitiesReported && r.NumberOfFatalities == null),
            Arg.Any<CancellationToken>());
    }

    private CreateAccidentReportCommand Command() => new()
    {
        TruckId = _truck.Id,
        DriverId = _driver.Id,
        Type = AccidentType.Collision,
        Severity = AccidentSeverity.Minor,
        AccidentDateTime = DateTime.UtcNow,
        Location = "I-35, Dallas, TX",
        Description = "Rear-ended at a light"
    };
}
