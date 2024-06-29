namespace Logistics.Application.Tenant.Commands;

internal sealed class RemoveEmployeeRoleHandler : RequestHandler<RemoveRoleFromEmployeeCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public RemoveEmployeeRoleHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }
    
    protected override async Task<Result> HandleValidated(
        RemoveRoleFromEmployeeCommand req, CancellationToken cancellationToken)
    {
        req.Role = req.Role.ToLower();
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (employee is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        var tenantRole = await _tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.Role);

        if (tenantRole is null)
        {
            return Result.Fail("Could not find the specified role name");
        }
        
        employee.Roles.Remove(tenantRole);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
