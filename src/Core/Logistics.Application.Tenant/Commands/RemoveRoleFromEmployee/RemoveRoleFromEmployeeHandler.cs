namespace Logistics.Application.Tenant.Commands;

internal sealed class RemoveEmployeeRoleHandler : RequestHandler<RemoveRoleFromEmployeeCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public RemoveEmployeeRoleHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<ResponseResult> HandleValidated(
        RemoveRoleFromEmployeeCommand req, CancellationToken cancellationToken)
    {
        req.Role = req.Role?.ToLower();
        var employee = await _tenantRepository.GetAsync<Employee>(req.UserId);

        if (employee == null)
            return ResponseResult.CreateError("Could not find the specified user");

        var tenantRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == req.Role);
        
        if (tenantRole == null)
            return ResponseResult.CreateError("Could not find the specified role name");

        employee.Roles.Remove(tenantRole);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
