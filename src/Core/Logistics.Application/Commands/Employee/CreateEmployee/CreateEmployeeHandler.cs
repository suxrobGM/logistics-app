using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateEmployeeHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    INotificationService notificationService)
    : IAppRequestHandler<CreateEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        CreateEmployeeCommand req, CancellationToken ct)
    {
        var existingEmployee = await tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (existingEmployee is not null)
        {
            return Result.Fail("Employee already exists");
        }

        var user = await masterUow.Repository<User>().GetByIdAsync(req.UserId);

        if (user is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        var tenantRole = await tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.Role);
        var tenant = tenantUow.GetCurrentTenant();

        user.Tenant = tenant;
        var employee = Employee.CreateEmployeeFromUser(user, req.Salary, req.SalaryType);

        if (tenantRole is not null)
        {
            employee.Role = tenantRole;
        }

        await tenantUow.Repository<Employee>().AddAsync(employee);
        masterUow.Repository<User>().Update(user);

        await masterUow.SaveChangesAsync();
        await tenantUow.SaveChangesAsync();

        await notificationService.SendNotificationAsync("New Employee",
            $"A new employee '{employee.GetFullName()}' has joined. Role is '{tenantRole?.DisplayName}'");
        return Result.Ok();
    }
}
