using Logistics.Application.Modules.Compliance.Accidents.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Compliance.Accidents;

public class CreateAccidentReportHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<AccidentReport, Guid> _reportRepo = Substitute.For<ITenantRepository<AccidentReport, Guid>>();
    private readonly Truck _truck = TestData.Truck();
    private readonly Employee _driver = TestData.Driver();
    private readonly CreateAccidentReportHandler _sut;

    public CreateAccidentReportHandlerTests()
    {
        var truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
        truckRepo.GetByIdAsync(_truck.Id, Arg.Any<CancellationToken>()).Returns(_truck);
        var employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
        employeeRepo.GetByIdAsync(_driver.Id, Arg.Any<CancellationToken>()).Returns(_driver);

        _tenantUow.Repository<Truck>().Returns(truckRepo);
        _tenantUow.Repository<Employee>().Returns(employeeRepo);
        _tenantUow.Repository<AccidentReport>().Returns(_reportRepo);
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

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _reportRepo.Received(1).AddAsync(
            Arg.Is<AccidentReport>(r =>
                r.FatalitiesReported
                && r.NumberOfFatalities == 2
                && r.VehicleTowed
                && r.TowCompany == "Ace Towing"
                && r.HazmatInvolved
                && r.HazmatDescription == "Diesel spill"),
            Arg.Any<CancellationToken>());
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
