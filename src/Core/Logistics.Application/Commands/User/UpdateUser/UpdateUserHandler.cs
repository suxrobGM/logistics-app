using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class UpdateUserHandler : RequestHandler<UpdateUserCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateUserHandler(
        IMasterUnityOfWork masterUow,
        ITenantUnityOfWork tenantUow)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateUserCommand req, CancellationToken cancellationToken)
    {
        var user = await _masterUow.Repository<User>().GetByIdAsync(req.Id);

        if (user is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        if (!string.IsNullOrEmpty(req.FirstName))
        {
            user.FirstName = req.FirstName;
        }

        if (!string.IsNullOrEmpty(req.LastName))
        {
            user.LastName = req.LastName;
        }

        if (!string.IsNullOrEmpty(req.PhoneNumber))
        {
            user.PhoneNumber = req.PhoneNumber;
        }

        var tenantIds = user.GetJoinedTenantIds();

        foreach (var tenantId in tenantIds)
        {
            await UpdateTenantEmployeeDataAsync(tenantId, user);
        }
        
        _masterUow.Repository<User>().Update(user);
        await _masterUow.SaveChangesAsync();
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }

    private async Task UpdateTenantEmployeeDataAsync(string tenantId, User user)
    {
        _tenantUow.SetCurrentTenantById(tenantId);
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(user.Id);

        if (employee is null)
        {
            return;
        }

        employee.FirstName = user.FirstName;
        employee.LastName = user.LastName;
        employee.Email = user.Email;
        employee.PhoneNumber = user.PhoneNumber;
        _tenantUow.Repository<Employee>().Update(employee);
    }
}
