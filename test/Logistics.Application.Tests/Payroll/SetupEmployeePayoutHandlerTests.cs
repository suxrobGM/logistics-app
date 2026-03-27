using Logistics.Application.Commands;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Stripe;
using Xunit;

namespace Logistics.Application.Tests.Payroll;

public class SetupEmployeePayoutHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IStripeConnectService stripeConnectService = Substitute.For<IStripeConnectService>();
    private readonly ILogger<SetupEmployeePayoutHandler> logger = NullLogger<SetupEmployeePayoutHandler>.Instance;

    private readonly ITenantRepository<Employee, Guid> employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();

    private readonly SetupEmployeePayoutHandler sut;

    public SetupEmployeePayoutHandlerTests()
    {
        tenantUow.Repository<Employee>().Returns(employeeRepo);
        sut = new SetupEmployeePayoutHandler(tenantUow, stripeConnectService, logger);
    }

    private static Employee CreateEmployee(string? stripeAccountId = null)
    {
        return new Employee
        {
            Email = "driver@test.com",
            FirstName = "John",
            LastName = "Doe",
            StripeConnectedAccountId = stripeAccountId
        };
    }

    [Fact]
    public async Task Handle_NewEmployee_CreatesStripeAccount()
    {
        var employee = CreateEmployee();
        employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        stripeConnectService.CreateEmployeeConnectedAccountAsync(employee)
            .Returns(new Account { Id = "acct_new_123" });

        var result = await sut.Handle(
            new SetupEmployeePayoutCommand { EmployeeId = employee.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("acct_new_123", employee.StripeConnectedAccountId);
        employeeRepo.Received(1).Update(employee);
        await tenantUow.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadySetUp_ReturnsOkWithoutCreating()
    {
        var employee = CreateEmployee(stripeAccountId: "acct_existing");
        employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);

        var result = await sut.Handle(
            new SetupEmployeePayoutCommand { EmployeeId = employee.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await stripeConnectService.DidNotReceive().CreateEmployeeConnectedAccountAsync(Arg.Any<Employee>());
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_ReturnsFailure()
    {
        employeeRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var result = await sut.Handle(
            new SetupEmployeePayoutCommand { EmployeeId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("not find", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_StripeFailure_ReturnsError()
    {
        var employee = CreateEmployee();
        employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        stripeConnectService.CreateEmployeeConnectedAccountAsync(employee)
            .ThrowsAsync(new StripeException("Account creation failed"));

        var result = await sut.Handle(
            new SetupEmployeePayoutCommand { EmployeeId = employee.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Account creation failed", result.Error!);
        Assert.Null(employee.StripeConnectedAccountId);
    }
}
