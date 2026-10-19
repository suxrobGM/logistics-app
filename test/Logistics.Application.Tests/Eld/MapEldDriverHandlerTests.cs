using Logistics.Application.Modules.Compliance.Eld.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class MapEldDriverHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly MapEldDriverHandler _sut;

    public MapEldDriverHandlerTests()
    {
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _sut = new MapEldDriverHandler(_tenantUow, NullLogger<MapEldDriverHandler>.Instance);
    }

    [Fact]
    public async Task Handle_TerminatedEmployee_FailsWithoutSaving()
    {
        var employee = new Employee
        {
            Email = "gone@test.com",
            FirstName = "Gone",
            LastName = "Driver",
            Salary = Money.Zero("USD"),
            Status = EmployeeStatus.Terminated
        };
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);

        var result = await _sut.Handle(new MapEldDriverCommand
        {
            EmployeeId = employee.Id,
            ProviderType = EldProviderType.Samsara,
            ExternalDriverId = "ext-1"
        }, default);

        Assert.False(result.IsSuccess);
        Assert.Contains("terminated", result.Error);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
