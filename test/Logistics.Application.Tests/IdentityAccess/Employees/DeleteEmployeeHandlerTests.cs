using System.Linq.Expressions;
using Logistics.Application.Modules.IdentityAccess.Employees.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Employees;

public class DeleteEmployeeHandlerTests
{
    [Fact]
    public async Task Handle_RemovesTheEmployeesTelegramChats()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        var masterUow = Substitute.For<IMasterUnitOfWork>();
        tenantUow.GetCurrentTenant().Returns(TestTenant.Create());

        var employee = new Employee
        {
            Email = "d@example.com",
            FirstName = "D",
            LastName = "River",
            Salary = new Money { Amount = 0, Currency = "USD" }
        };
        var employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
        employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        tenantUow.Repository<Employee>().Returns(employeeRepo);

        var loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
        loadRepo.Query().Returns(new List<Load>().BuildMock());
        tenantUow.Repository<Load>().Returns(loadRepo);

        var chat = new TelegramChat { ChatId = 42, UserId = employee.Id };
        var chatRepo = Substitute.For<ITenantRepository<TelegramChat, Guid>>();
        chatRepo.GetListAsync(Arg.Any<Expression<Func<TelegramChat, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([chat]);
        tenantUow.Repository<TelegramChat>().Returns(chatRepo);

        var result = await new DeleteEmployeeHandler(masterUow, tenantUow).Handle(
            new DeleteEmployeeCommand { UserId = employee.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        chatRepo.Received(1).Delete(chat);
        employeeRepo.Received(1).Delete(employee);
    }
}
