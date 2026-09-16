using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Logistics.Application.Abstractions.Payments.Stripe;
using Address = Logistics.Domain.Primitives.ValueObjects.Address;
using StripeAccount = Stripe.Account;
using StripeException = Stripe.StripeException;
using Logistics.Application.Modules.Financial.Payroll.Commands;

namespace Logistics.Application.Tests.Payroll;

public class SetupEmployeePayoutHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IStripeConnectService _stripeConnectService = Substitute.For<IStripeConnectService>();
    private readonly ILogger<SetupEmployeePayoutHandler> _logger = NullLogger<SetupEmployeePayoutHandler>.Instance;

    private readonly ITenantRepository<Employee, Guid> _employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();

    private readonly SetupEmployeePayoutHandler _sut;

    public SetupEmployeePayoutHandlerTests()
    {
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _tenantUow.GetCurrentTenant().Returns(CreateTenant());
        _sut = new SetupEmployeePayoutHandler(_tenantUow, _stripeConnectService, _logger);
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

    private static Tenant CreateTenant() => new()
    {
        Name = "test",
        ConnectionString = "test",
        BillingEmail = "billing@test.com",
        CompanyAddress = new Address
        {
            Line1 = "1 Test St",
            City = "Test",
            ZipCode = "00000",
            State = "NA",
            Country = "US"
        }
    };

    [Fact]
    public async Task Handle_NewEmployee_CreatesStripeAccount()
    {
        var employee = CreateEmployee();
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _stripeConnectService.CreateEmployeeConnectedAccountAsync(employee, Arg.Any<Address>())
            .Returns(new StripeAccount { Id = "acct_new_123" });

        var result = await _sut.Handle(
            new SetupEmployeePayoutCommand { EmployeeId = employee.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("acct_new_123", employee.StripeConnectedAccountId);
        _employeeRepo.Received(1).Update(employee);
        await _tenantUow.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadySetUp_ReturnsOkWithoutCreating()
    {
        var employee = CreateEmployee(stripeAccountId: "acct_existing");
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);

        var result = await _sut.Handle(
            new SetupEmployeePayoutCommand { EmployeeId = employee.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _stripeConnectService.DidNotReceive()
            .CreateEmployeeConnectedAccountAsync(Arg.Any<Employee>(), Arg.Any<Address>());
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_ReturnsFailure()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var result = await _sut.Handle(
            new SetupEmployeePayoutCommand { EmployeeId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("not find", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_StripeFailure_ReturnsError()
    {
        var employee = CreateEmployee();
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _stripeConnectService.CreateEmployeeConnectedAccountAsync(employee, Arg.Any<Address>())
            .ThrowsAsync(new StripeException("Account creation failed"));

        var result = await _sut.Handle(
            new SetupEmployeePayoutCommand { EmployeeId = employee.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Account creation failed", result.Error!);
        Assert.Null(employee.StripeConnectedAccountId);
    }
}
