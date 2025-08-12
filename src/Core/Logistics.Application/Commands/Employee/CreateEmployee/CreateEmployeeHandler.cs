using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateEmployeeHandler : IAppRequestHandler<CreateEmployeeCommand, Result>
{
    private readonly IMasterUnitOfWork _masterUow;
    private readonly INotificationService _notificationService;
    private readonly ITenantUnitOfWork _tenantUow;

    public CreateEmployeeHandler(
        IMasterUnitOfWork masterUow,
        ITenantUnitOfWork tenantUow,
        INotificationService notificationService)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(
        CreateEmployeeCommand req, CancellationToken ct)
    {
        var existingEmployee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (existingEmployee is not null)
        {
            return Result.Fail("Employee already exists");
        }

        var user = await _masterUow.Repository<User>().GetByIdAsync(req.UserId);

        if (user is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        var tenantRole = await _tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.Role);
        var tenant = _tenantUow.GetCurrentTenant();

        user.Tenant = tenant;
        var employee = Employee.CreateEmployeeFromUser(user, req.Salary, req.SalaryType);

        if (tenantRole is not null)
        {
            employee.Roles.Add(tenantRole);
        }

        await _tenantUow.Repository<Employee>().AddAsync(employee);
        _masterUow.Repository<User>().Update(user);

        await _masterUow.SaveChangesAsync();
        await _tenantUow.SaveChangesAsync();

        await _notificationService.SendNotificationAsync("New Employee",
            $"A new employee '{employee.GetFullName()}' has joined. Role is '{tenantRole?.DisplayName}'");
        return Result.Ok();
    }
}
