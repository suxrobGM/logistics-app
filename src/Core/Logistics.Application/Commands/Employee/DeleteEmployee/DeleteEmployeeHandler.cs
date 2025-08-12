using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteEmployeeHandler : RequestHandler<DeleteEmployeeCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly ITenantUnityOfWork _tenantUow;

    public DeleteEmployeeHandler(
        IMasterUnityOfWork masterUow,
        ITenantUnityOfWork tenantUow)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        DeleteEmployeeCommand req, CancellationToken cancellationToken)
    {
        var tenant = _tenantUow.GetCurrentTenant();
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (employee is null)
        {
            return Result.Fail($"Could not find employee with ID {req.UserId}");
        }

        var user = await _masterUow.Repository<User>().GetByIdAsync(employee.Id);

        // Remove tenant from user if it matches the current tenant
        if (user?.Tenant != null && user.Tenant.Id == tenant.Id)
        {
            user.Tenant = null;
        }

        var employeeLoads = _tenantUow.Repository<Load>().ApplySpecification(new GetEmployeeLoads(employee.Id));
        // var truck = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == employee.Id);

        foreach (var load in employeeLoads)
        {
            if (load.AssignedDispatcherId == employee.Id)
            {
                load.AssignedDispatcher = null;
            }
        }

        _tenantUow.Repository<Employee>().Delete(employee);
        await _tenantUow.SaveChangesAsync();
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
